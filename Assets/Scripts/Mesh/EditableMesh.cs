using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Meshes;
#nullable enable

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditableMesh : MonoBehaviour
{
    EditableMeshImpl meshInternal;

    [SerializeField]
    Mesh? CopyFromMesh;

    public EditableMeshImpl MeshInternal { get => meshInternal; private set => meshInternal = value; }

    private void OnEnable()
    {

        //meshInternal = default;

        if (CopyFromMesh != null)
        {
            meshInternal = default;
            Debug.Log($"Vertex count: {CopyFromMesh.vertexCount}");
            GetComponent<MeshFilter>().mesh = meshInternal.CopySetup(CopyFromMesh);
            
        }
        else
        {
            var mesh = new Mesh
            {
                name = "Flat quad"
            };
            meshInternal.Setup(mesh);
            var vert1 = float3(0f, 0f, 0f);
            var vert2 = float3(1f, 0f, 0f);
            var vert3 = float3(1f, 1f, 0f);
            var vert4 = float3(0f, 1f, 0f);
            meshInternal.AddFace(vert1, vert2, vert3, vert4);
            GetComponent<MeshFilter>().mesh = mesh;
        }

    }

}



namespace Meshes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Stream0
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public float4 tangent; // 16 bytes
        public float2 texCoord0; // 8 bytes
    }
    public struct EditableMeshImpl
    {
        private Mesh Mesh;

        private List<Vertex> _vertices;
        private List<Face> _faces;

        private int _maxVerts => defaultMaxVerts;
        private int _vertexCountInternal;
        private int _maxFaces => defaultMaxTris;

        private static readonly int defaultMaxVerts = ushort.MaxValue;
        private static readonly int defaultMaxTris = ushort.MaxValue / 3;

        public int VertexCount { get => _vertices.Count; }
        public int TriangleCount { get; private set; }

        public Vector3[] Vertices
        {
            get => _vertices.Take(VertexCount).Select(vertex => (Vector3)vertex.position).ToArray();
        }

        /// <summary>
        /// Create a new EditableMesh from scratch
        /// </summary>
        /// <param name="mesh">Container mesh to write to</param>
        public void Setup(Mesh mesh)
        {
            Mesh = mesh;

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
            _vertices = new List<Vertex>(defaultMaxVerts);
            _faces = new List<Face>(defaultMaxTris/3);

            _vertexCountInternal = 0;
            TriangleCount = 0;

            var stream0 = meshData.GetVertexData<Stream0>();
            var triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);

            for (int i = 0; i < stream0.Length; ++i)
            {
                stream0[i] = default;
            }
            for (int i = 0; i < triangles.Length; ++i)
            {
                triangles[i] = int3(0, 0, 0);
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
            Mesh = new Mesh
            {
                name = mesh.name,
            };
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var uvs = mesh.uv;
            Setup(Mesh);
            for (int i = 0; i < vertices.Length; ++i)
            {
                Debug.Log($"idx: {i}\n\tpos: {vertices[i]}\n\tnorm: {normals[i]}\n");
                AddVertex(vertices[i], normals[i], tangents[i], uvs[i]);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Debug.Log($"idx: {i}\n\tindices: {triangles[i]}, {triangles[i+1]}, {triangles[i+2]}\n");
                AddTriangle(int3(triangles[i], triangles[i + 1], triangles[i + 2]));
            }
            return Mesh;
        }

        public Vector3[] GetFaces()
        {
            // TODO
            throw new NotImplementedException { };
        }

        /// <summary>
        /// Move a vertex of the given index to the specified position
        /// </summary>
        /// <param name="index">index of the vertex to move</param>
        /// <param name="position">position to move to</param>
        public void MoveVertex(int index, float3 position)
        {
            // need to abstract real index
            var vert = _vertices[index];
            vert.position = position;
            _vertices[index] = vert;
            //_vertices[index].position = position;
            var single_vert = new Stream0
            {
                position = vert.position,
                normal = vert.normal,
                tangent = vert.tangent,
                texCoord0 = vert.texCoord0,
            };
            NativeArray<Stream0> buffer = new NativeArray<Stream0>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            buffer[0] = single_vert;
            Mesh.SetVertexBufferData<Stream0>(buffer, 0, index, 1);
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
            var vert = Vertex.Create(position, normal, tangent, texCoord0);
            vert.indices.Add(_vertexCountInternal);
            _vertices.Add(vert);
            Stream0 vert_int = (Stream0)vert;
            NativeArray<Stream0> buffer = new NativeArray<Stream0>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            buffer[0] = vert_int;
            Mesh.SetVertexBufferData<Stream0>(buffer, 0, _vertexCountInternal, 1);
            buffer.Dispose();
            _vertexCountInternal++;
            return VertexCount - 1;
        }


        public int AddVertexOn(int index)
        {
            return AddVertex(_vertices[index].position);
        }

        public void DeleteVertex()
        {
            // delete the vertex

            // shift the vertex buffer left

            // update the index buffer values

            // copy all of the updated values to the 

            throw new NotImplementedException { };
        }

        public void DeleteFace()
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
        public void AddFace(int4 vertices)
        {
            List<Vertex> face_verts = new List<Vertex> {
                _vertices[vertices.x],
                _vertices[vertices.y],
                _vertices[vertices.z],
                _vertices[vertices.w]
            };
            var face = new Face(face_verts);
            _faces.Add(face);
            var buf = face.ToStream();
            var buffer = new NativeArray<int3>(buf, Allocator.Temp);

            Mesh.SetIndexBufferData<int3>(buffer, 0, TriangleCount, 2);
            buffer.Dispose();
            TriangleCount += 2;
        }

        public void AddTriangle(int3 vertices)
        {
            List<Vertex> face_verts = new List<Vertex> {
                _vertices[vertices.x],
                _vertices[vertices.y],
                _vertices[vertices.z]
            };
            var face = new Face(face_verts);
            _faces.Add(face);
            var buf = face.ToStream();
            var buffer = new NativeArray<int3>(buf, Allocator.Temp);
            Mesh.SetIndexBufferData<int3>(buffer, 0, TriangleCount, 1);
            buffer.Dispose();
            TriangleCount += 1;
        }

        public void Extrude()
        {
            throw new NotImplementedException { };
        }

        public Vertex? FindByPosition()
        {
            throw new NotImplementedException { };
        }
    }

    public class Vertex
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public float4 tangent; // 16 bytes
        public float2 texCoord0; // 8 bytes
        // maybe add bookkeeping fields?
        public List<int> indices = new List<int>();
        public List<Face> faces = new List<Face>();

        public static Vertex Create(float3 position, float3 normal, float4 tangent, float2 texCoord0)
        {
            return new Vertex
            {
                position = position,
                normal = normal,
                tangent = tangent,
                texCoord0 = texCoord0,
                indices = new List<int>(),
            };
        }

        public static explicit operator Stream0(Vertex vert)
        {
            return new Stream0
            {
                position = vert.position,
                normal = vert.normal,
                tangent = vert.tangent,
                texCoord0 = vert.texCoord0,
            };
        }

        public int GetIndex(Face face)
        {
            // TODO: smarter handling
            return indices.First();
            //return faces.IndexOf(face);
        }

    }

    public struct Edge
    {

    }

    public class Face
    {
        // Assume that the list of vertices is clockwise on the face
        public List<Vertex> vertices = new List<Vertex>(4);
        //public float3 normal;

        private static readonly int3[] empty = new int3[0];
        private static readonly int3[] degenerate = { int3(0, 0, 0) };

        public static int3[] Empty => empty;

        public Face()
        {

        }

        public Face(List<Vertex> vertices)
        {
            this.vertices = vertices;
        }

        public void AddVertex()
        {
            throw new NotImplementedException { };
        }

        

        /// <summary>
        /// Triangulate the face and return the triangle indices
        /// </summary>
        /// <returns>Triangle indices</returns>
        public int3[] ToStream()
        { 
            if (vertices.Count < 3)
            {
                return empty;
            } else
            {
                var indices = new int3[vertices.Count - 2];
                var ind0 = vertices[0].GetIndex(this);
                for (int i = 0; i < vertices.Count - 2; i++)
                {
                    indices[i] = int3(ind0, vertices[i + 1].GetIndex(this), vertices[i + 2].GetIndex(this));
                }
                return indices;
            }
        }
    }
}