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
    public partial struct UMesh
    {

        private Mesh mesh;

        public Mesh Mesh { get => mesh; set => CopySetup(value); }
        public string Name { get => mesh.name; set => mesh.name = value; }

        public List<Vertex> Vertices;
        /// <summary>
        /// Currently unstable, do not rely on this
        /// </summary>
        public List<Edge> Edges;
        public List<Face> Faces;

        private int internalVertexCount;
        private int internalVertexCountMax;
        private int internalIndexCount;
        private int internalTriangleCount => internalIndexCount / 3;
        private int internalIndexCountMax;

        private static readonly int absoluteMaxVerts = 2 << 19;
        private static readonly int absoluteMaxIndices = 2 << 19;

        private static readonly int initialMaxVerts = 320;
        private static readonly int initialMaxTriangles = 320;

        public int VertexCount { get => Vertices.Count; }
        public int FaceCount { get => Faces.Count; }
        /// <summary>
        /// Currently unstable, do not rely on this
        /// </summary>
        public int EdgeCount { get => Edges.Count; }

        private ExtrusionHelper extrusionHelper;

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

            // initialize safe garbage here
            Vertices = new List<Vertex>(initialMaxVerts / 2);
            Faces = new List<Face>(initialMaxVerts / 2);
            Edges = new List<Edge>(initialMaxVerts);

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            Setup(meshData, initialMaxVerts, initialMaxTriangles);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
            InitializeHelperStructures();
        }

        private void Setup(Mesh.MeshData meshData, int vertexCount, int triangleCount)
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

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            vertexAttributes.Dispose();

            meshData.SetIndexBufferParams(triangleCount * 3, IndexFormat.UInt32);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triangleCount * 3),
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            internalVertexCountMax = vertexCount;
            internalIndexCountMax = triangleCount * 3;

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

        private void InitializeHelperStructures()
        {
            extrusionHelper.Setup();
        }

        private void ResizeInternalBuffers(int currentVertexCount, int currentIndexCount)
        {
            if (currentVertexCount > absoluteMaxVerts || currentIndexCount > absoluteMaxIndices)
            {
                // if the required vertices are too large, give up
                throw new InvalidOperationException(
                    "Number of vertices or triangle indices has exceeded the maximum allowed" +
                    "by the API, this usually indicates that the previous operation has required" +
                    "too many new vertices to be created and the operation was prevented");
            }
            // set up mesh again

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];
            Setup(meshData, currentVertexCount, currentIndexCount);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
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
            InitializeHelperStructures();
            return this.mesh;
        }

        public void Combine(UMesh other)
        {
            Vertices.AddRange(other.Vertices);
            Faces.AddRange(other.Faces);
            other.Vertices.Clear();
            other.Faces.Clear();
        }

        public UMesh Split(List<Vertex> vertices)
        {
            throw new NotImplementedException { };
        }

        public UMesh DeepCopy()
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
            internalVertexCount = i;

            i = 0;
            foreach (Face face in Faces)
            {
                i += face.TriangleCount;
            }
            internalIndexCount =  i * 3;
        }

        /// <summary>
        /// Call this method after modifying the mesh to update indices and display all of the changes
        /// </summary>
        public void WriteAllToMesh()
        {
            int previousVertexCount = internalVertexCount;
            int previousIndexCount = internalIndexCount;
            // figure out the indices
            OptimizeIndices();
            // resize the vertex and index buffers if needed
            if (internalVertexCount > internalVertexCountMax || internalIndexCount > internalIndexCountMax)
            {
                ResizeInternalBuffers(internalVertexCount, internalIndexCount);
            }
            // write vertices 
            NativeArray<Stream0> vertexStream = new NativeArray<Stream0>(internalVertexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            foreach (Vertex vertex in Vertices)
            {
                vertex.WriteToStream(ref vertexStream);
            }
            mesh.SetVertexBufferData<Stream0>(vertexStream, 0, 0, internalVertexCount,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            vertexStream.Dispose();
            NativeArray<int3> indexStream = new NativeArray<int3>(internalIndexCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int i = 0;
            foreach(Face face in Faces)
            {
                i = face.WriteToStream(ref indexStream, i);
            }
            mesh.SetIndexBufferData<int3>(indexStream, 0, 0, internalTriangleCount,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);

            // If any deletions have occurred, zero out the remainder
            DeletePadding(internalVertexCount, previousVertexCount, internalIndexCount, previousIndexCount);
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

        public void Dispose()
        {
            Vertices.Clear();
            Edges.Clear();
            Faces.Clear();
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