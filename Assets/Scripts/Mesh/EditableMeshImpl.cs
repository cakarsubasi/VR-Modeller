using Unity.Collections;
using Unity.Mathematics;
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
        private int _indexCountInternal;

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

            int[] vertexmap = new int[vertices.Length];

            for (int i = 0; i < vertices.Length; ++i)
            {
                int trueIndex = AddVertexPossiblyOverlapping(vertices[i], normals[i], tangents[i], uvs[i]);
                vertexmap[i] = trueIndex;
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int ind0 = vertexmap[triangles[i]];
                int ind1 = vertexmap[triangles[i + 1]];
                int ind2 = vertexmap[triangles[i + 2]];
                float2[] uv0s = { uvs[triangles[i]], uvs[triangles[i + 1]], uvs[triangles[i + 2]]};
                AddTriangle(int3(ind0, ind1, ind2), uv0s);
            }
            WriteAllToMesh();
            return this.mesh;
        }

        /// <summary>
        /// Move a vertex of the given index to the specified position
        /// </summary>
        /// <param name="index">index of the vertex to move</param>
        /// <param name="position">position to move to</param>
        public void MoveVertex(int index, float3 position)
        {
            // need to abstract real index
            Vertices[index].Position = position;
            //_vertices[index].position = position;
            Stream0[] stream = Vertices[index].ToStream();
            int[] indices = Vertices[index].Indices();
            NativeArray<Stream0> buffer = new(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < indices.Length; ++i)
            {
                buffer[0] = stream[i];
                mesh.SetVertexBufferData<Stream0>(buffer, 0, indices[i], 1);
            }
            buffer.Dispose();
        }

        /// <summary>
        /// Add Vertex to the given position
        /// </summary>
        /// <param name="position">position of the vertex</param>
        /// <returns>index of the vertex</returns>
        public int AddVertex(float3 position)
        {
            return AddVertex(position, normal: back(), tangent: float4(1f, 0f, 0f, -1f), float2(0f, 0f));
        }

        public int AddVertexPossiblyOverlapping(float3 position, float3 normal, float4 tangent, float2 texCoord0)
        {
            IndexedVertex vertMaybe = FindByPosition(position);
            if (vertMaybe.vertex == null)
            {
                return AddVertex(position, normal, tangent, texCoord0);
            }
            else
            {
                return vertMaybe.index;
            }
        }


        /// <summary>
        /// Add Vertex to the given position with the given normal and tangent
        /// </summary>
        /// <param name="position">position to add to</param>
        /// <param name="normal">normal of the vertex</param>
        /// <param name="tangent">tangent of the vertex</param>
        /// <param name="texCoord0">UV coordinate of the vertex</param>
        /// <returns>index of the vertex</returns>
        public int AddVertex(float3 position, float3 normal, float4 tangent, float2 texCoord0)
        {
            var vert = Vertex.Create(position, normal, tangent, texCoord0, _vertexCountInternal);
            Vertices.Add(vert);
            return VertexCount - 1;
        }

        public int AddVertexOn(int index)
        {
            return AddVertex(Vertices[index].Position);
        }

        public enum TransformType
        {
            BoundingBoxCenter,
            IndividualCenter,
            // ...
        }

        public void TransformVertices(List<int> vertices, Matrix4x4 transform, TransformType type)
        {
            throw new NotImplementedException { };
        }


        public void SetFace()
        {
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Add a quad by creating four vertices
        /// </summary>
        /// <param name="pos1"></param>
        /// <param name="pos2"></param>
        /// <param name="pos3"></param>
        /// <param name="pos4"></param>
        public void AddFace(float3 pos1, float3 pos2, float3 pos3, float3 pos4)
        {
            var indices = new int4
            {
                x = AddVertex(pos1),
                y = AddVertex(pos2),
                z = AddVertex(pos3),
                w = AddVertex(pos4),
            };
            AddFace(indices);
        }

        /// <summary>
        /// Add a quad between the given four vertices
        /// </summary>
        /// <param name="vertices"></param>
        public void AddFace(int4 vertices, float2[]? uv0s = null)
        {
            List<Vertex> face_verts = new()
            {
                this.Vertices[vertices.x],
                this.Vertices[vertices.y],
                this.Vertices[vertices.z],
                this.Vertices[vertices.w]
            };
            if (uv0s == null)
            {
                uv0s = new float2[4];
            }
            var face = new Face(face_verts, uv0s.ToList());
            Faces.Add(face);
        }

        public void AddTriangle(int3 vertices, float2[]? uv0s = null)
        {
            List<Vertex> face_verts = new()
            {
                this.Vertices[vertices.x],
                this.Vertices[vertices.y],
                this.Vertices[vertices.z]
            };
            if (uv0s == null)
            {
                uv0s = new float2[3];
            }
            var face = new Face(face_verts, uv0s.ToList());
            Faces.Add(face);
        }

        public void Extrude()
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
            OptimizeIndices();
            //Stream0[] vertexStream = new Stream0[_vertexCountInternal];
            NativeArray<Stream0> vertexStream = new NativeArray<Stream0>(_vertexCountInternal, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            foreach (Vertex vertex in Vertices)
            {
                vertex.WriteToStream(ref vertexStream);
            }
            //Stream0[] vertexStream = Vertices.SelectMany(vertex => vertex.ToStream()).ToArray();
            mesh.SetVertexBufferData<Stream0>(vertexStream, 0, 0, _vertexCountInternal,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            vertexStream.Dispose();

            NativeArray<int3> indexStream = new NativeArray<int3>(_indexCountInternal, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            int i = 0;
            foreach(Face face in Faces)
            {
                i = face.WriteToStream(ref indexStream, i);
            }
            //int3[] triangleStream = Faces.SelectMany(face => face.ToStream()).ToArray();
            mesh.SetIndexBufferData<int3>(indexStream, 0, 0, _indexCountInternal,
                flags: MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
        }

        public void UnsafeWriteVertexToMesh(int index)
        {
            UnsafeWriteVerticesToMesh(new List<Vertex>() { Vertices[index] });
        }

        /// <summary>
        /// Write the given vertices to the mesh without validating indices. This is useful for applying
        /// just movement for instance. If called after adding or deleting meshes or faces, it may result
        /// in broken geometry.
        /// </summary>
        /// <param name="verts">vertices to write</param>
        public void UnsafeWriteVerticesToMesh(List<Vertex> verts)
        {
            foreach (var vert in verts)
            {
                Stream0[] stream = vert.ToStream();
                int[] indices = vert.Indices();
                NativeArray<Stream0> buffer = new(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                for (int i = 0; i < indices.Length; ++i)
                {
                    buffer[0] = stream[i];
                    Mesh.SetVertexBufferData<Stream0>(buffer, 0, indices[i], 1);
                }
                buffer.Dispose();
            }
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

        public void Triangulate()
        {
            throw new NotImplementedException { };
        }

        public void TrianglesToQuads()
        {
            throw new NotImplementedException { };
        }

        public void RecalculateBounds()
        {
            throw new NotImplementedException { };
        }

        private IndexedVertex FindByPosition(float3 position)
        {
            var idx = Vertices.FindIndex(vertex => vertex.Position.Equals(position));
            Vertex? vertex = null;
            if (idx != -1)
                vertex = Vertices[idx];
            return new IndexedVertex
            {
                index = idx,
                vertex = vertex
            };
        }

        private List<Vertex> FindAllByPosition(float3 position)
        {
            return Vertices.FindAll(vert => vert.Position.Equals(position)).ToList();
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