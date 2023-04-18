using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
#nullable enable

using static Unity.Mathematics.math;

namespace Meshes
{
    public partial struct EditableMeshImpl
    {
        public struct IndexedVertex
        {
            public int index;
            public Vertex? vertex;
        }

        private Mesh mesh;

        public Mesh Mesh { get => mesh; set => CopySetup(value); }

        public List<Vertex> Vertices;
        public List<Face> Faces;

        private int _vertexCountInternal;
        private int vertexCountMaximum;
        private int _indexCountInternal;
        private int indexCountMaximum;

        private static readonly int defaultMaxVerts = ushort.MaxValue;
        private static readonly int defaultMaxTris = ushort.MaxValue / 3;

        public int VertexCount { get => Vertices.Count; }

        public int FaceCount { get => Faces.Count; }
        public int TriangleCount { get; private set; }

        public Vector3[] VertexLocations
        {
            get => Vertices.Select(vertex => (Vector3)vertex.Position).ToArray();
        }

        public Vector3[] FaceLocations
        {
            get => Faces.Select(face => (Vector3)face.Position).ToArray();
        }

        /// <summary>
        /// Create a new EditableMesh from scratch
        /// </summary>
        /// <param name="mesh">Container mesh to write to</param>
        public void Setup(Mesh mesh)
        {
            this.mesh = mesh;
            this.mesh.MarkDynamic();

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            Setup(meshData);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

        public void Setup(Mesh.MeshData meshData)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
    4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // positions
            vertexAttributes[0] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Position);
            // normals
            vertexAttributes[1] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Normal);
            // tangents
            vertexAttributes[2] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Tangent,
                dimension: 4);
            // uv coordinates
            vertexAttributes[3] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.TexCoord0,
                dimension: 2);

            meshData.SetVertexBufferParams(defaultMaxVerts, vertexAttributes);
            vertexAttributes.Dispose();

            meshData.SetIndexBufferParams(defaultMaxTris * 3, IndexFormat.UInt32);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, defaultMaxTris * 3),
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            // initialize safe garbage here
            Vertices = new List<Vertex>(defaultMaxVerts);
            Faces = new List<Face>(defaultMaxTris / 3);

            vertexCountMaximum = defaultMaxVerts;
            indexCountMaximum = defaultMaxTris * 3;

            _vertexCountInternal = 0;
            TriangleCount = 0;

            var stream0 = meshData.GetVertexData<Stream0>();
            var triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);

            for (int i = 0; i < stream0.Length; ++i)
            {
                stream0[i] = Stream0.degenerate;
            }
            for (int i = 0; i < triangles.Length; ++i)
            {
                triangles[i] = Triangle.degenerate;
            }
        }

        private void ResizeInternalBuffers(int currentVertexCount, int currentIndexCount)
        {
            // set up mesh again

            // if the required vertices are too large, give up
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Create an editable mesh by copying an existing mesh
        /// The original instance is not touched and a new instance is returned
        /// </summary>
        /// <param name="mesh">Mesh to copy from</param>
        /// <returns>A reference to the new mesh instance</returns>
        public Mesh CopySetup(Mesh mesh)
        {
            this.mesh = new Mesh
            {
                name = mesh.name,
            };
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var uvs = mesh.uv;
            Setup(this.mesh);

            Vertex[] vertexmap = new Vertex[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertexmap[i] = CreateVertexOrReturnReferenceIfExists(vertices[i], normals[i], tangents[i]);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                TriangleVerts verts = new(vertexmap[triangles[i]], vertexmap[triangles[i + 1]], vertexmap[triangles[i + 2]]);
                TriangleUVs uv0s = new(uvs[triangles[i]], uvs[triangles[i + 1]], uvs[triangles[i + 2]]);
                CreateTriangle(verts, uv0s);
            }
            RecalculateNormals();
            WriteAllToMesh();
            RecalculateBounds();
            return this.mesh;
        }

        public void Combine(EditableMeshImpl other)
        {
            Vertices.AddRange(other.Vertices);
            Faces.AddRange(other.Faces);
            other.Vertices.Clear();
            other.Faces.Clear();
        }

        public EditableMeshImpl Split(List<Vertex> vertices)
        {
            throw new NotImplementedException { };
        }

        public EditableMeshImpl DeepCopy()
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Optimize the in memory representation of the mesh for rendering.
        /// <br></br>
        /// Call after particularly complex operations
        /// </summary>
        public void OptimizeIndices()
        {
            int i = 0;
            foreach (Vertex vertex in Vertices)
            {
                i = vertex.OptimizeIndices(i);
            }
            _vertexCountInternal = i;

            i = 0;
            foreach (Face face in Faces)
            {
                i += face.TriangleCount;
            }
            _indexCountInternal =  i;
        }

        /// <summary>
        /// Call this method after modifying the mesh to update indices and display all of the changes
        /// </summary>
        public void WriteAllToMesh()
        {
            // figure out the indices
            OptimizeIndices();
            // resize the vertex and index buffers if needed
            if (_vertexCountInternal > vertexCountMaximum || _indexCountInternal > indexCountMaximum)
            {
                ResizeInternalBuffers(_vertexCountInternal, _indexCountInternal);
            }
            // write vertices 
            NativeArray<Stream0> vertexStream = new NativeArray<Stream0>(_vertexCountInternal, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            foreach (Vertex vertex in Vertices)
            {
                vertex.WriteToStream(ref vertexStream);
            }
            mesh.SetVertexBufferData<Stream0>(vertexStream, 0, 0, _vertexCountInternal,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            vertexStream.Dispose();
            NativeArray<int3> indexStream = new NativeArray<int3>(_indexCountInternal, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int i = 0;
            foreach(Face face in Faces)
            {
                i = face.WriteToStream(ref indexStream, i);
            }
            mesh.SetIndexBufferData<int3>(indexStream, 0, 0, _indexCountInternal,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
        }

        public void UnsafeWriteVertexToMesh(Vertex vertex)
        {
            UnsafeWriteVerticesToMesh(new List<Vertex>() { vertex });
        }

        /// <summary>
        /// Write the given vertices to the mesh without validating indices. This is useful for applying
        /// just movement for instance. If called after adding or deleting meshes or faces, it may result
        /// in broken geometry.
        /// </summary>
        /// <param name="verts">vertices to write</param>
        public void UnsafeWriteVerticesToMesh(List<Vertex> verts)
        {
            NativeArray<Stream0> buffer = new(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            foreach (var vert in verts)
            {
                Stream0[] stream = vert.ToStream();
                int[] indices = vert.Indices();

                for (int i = 0; i < indices.Length; ++i)
                {
                    buffer[0] = stream[i];
                    Mesh.SetVertexBufferData<Stream0>(buffer, 0, indices[i], 1);
                }
            }
            buffer.Dispose();
        }

        private void DeletePadding(int vertexStart, int vertexEnd, int triangleStart, int triangleEnd)
        {
            Stream0[] degenerateVert = { Stream0.degenerate };
            for (int i = vertexStart; i < vertexEnd; i++)
            {
                mesh.SetVertexBufferData<Stream0>(degenerateVert, 0, i, 1,
                    flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            }
            int3[] degenerateTriangle = { Triangle.degenerate };
            for (int i = triangleStart; i < triangleEnd; i++)
            {
                mesh.SetIndexBufferData<int3>(degenerateTriangle, 0, i, 1, 
                    flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            }
        }

        /// <summary>
        /// Recalculate the bounds for the renderer.
        /// </summary>
        public void RecalculateBounds()
        {
            Mesh.RecalculateBounds();
        }

        public override string ToString()
        {
            var str = $"{base.ToString()}: vertices: {VertexCount}, faces: {FaceCount}";

            foreach(Vertex vert in Vertices)
            {
                str = $"{str}\n{vert}";
            }
            str = $"{str}\nFaces:";
            foreach (Face face in Faces)
            {
                str = $"{str}\n{face}";
            }
            return str;
        }

    }

}