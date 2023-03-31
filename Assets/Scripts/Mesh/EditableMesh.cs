using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Meshes;
#nullable enable

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditableMesh : MonoBehaviour
{
    EditableMeshImpl meshInternal;

    [SerializeField]
    Mesh? CopyFromMesh;

    private void OnEnable()
    {
        meshInternal = default;
        if (CopyFromMesh != null)
        {
            meshInternal = default;
            meshInternal.CopySetup(CopyFromMesh);
            GetComponent<MeshFilter>().mesh = CopyFromMesh;
        } else
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
    struct Stream0
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public float4 tangent; // 16 bytes
        public float2 texCoord0; // 8 bytes
    }
    public struct EditableMeshImpl
    {
        private Mesh Mesh;

        private Vertex[] _vertices;
        private Face[] _faces;

        private int _maxVerts => defaultMaxVerts;
        private int _maxFaces => defaultMaxTris;

        private static readonly int defaultMaxVerts = ushort.MaxValue;
        private static readonly int defaultMaxTris = ushort.MaxValue / 3;

        public int VertexCount { get; private set; }
        public int TriangleCount { get; private set; }

        /// <summary>
        /// Create a new EditableMesh from scratch
        /// </summary>
        /// <param name="mesh">Container mesh to write to</param>
        public void Setup(Mesh mesh)
        {
            Mesh = mesh;

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

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
            _vertices = new Vertex[defaultMaxVerts];
            _faces = new Face[defaultMaxTris];
            VertexCount = 0;
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
            
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        }

        /// <summary>
        /// Create an editable mesh by copying an existing mesh
        /// </summary>
        /// <param name="mesh">Mesh to copy from</param>
        public void CopySetup(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var normals = mesh.normals;
            var tangents = mesh.tangents;
            var uvs = mesh.uv;
            Setup(mesh);
            for (int i = 0; i < vertices.Length; ++i)
            {
                AddVertex(vertices[i], normals[i], tangents[i], uvs[i]);
            }
            for (int i = 0; i < triangles.Length; i += 3)
            {
                AddTriangle(int3(triangles[i], triangles[i + 1], triangles[i + 2]));
            }

        }

        /// <summary>
        /// Get the position of vertices within the mesh, use the indexes to reference the vertices
        /// </summary>
        /// <returns>List of indexes of vertices</returns>
        public Vector3[] GetVertices()
        {
            var positions = new Vector3[_vertices.Length];
            for (int i = 0; i < _vertices.Length; ++i)
            {
                positions[i] = _vertices[i].position;
            }
            return positions;
        }

        public Vector3[] GetFaces()
        {
            // TODO
            return new Vector3[0];
        }

        /// <summary>
        /// Move a vertex of the given index to the specified position
        /// </summary>
        /// <param name="index">index of the vertex to move</param>
        /// <param name="position">position to move to</param>
        public void MoveVertex(int index, float3 position)
        {
            // need to abstract real index
            _vertices[index].position = position;
            var single_vert = new Stream0
            {
                position = _vertices[index].position,
                normal = _vertices[index].normal,
                tangent = _vertices[index].tangent,
                texCoord0 = _vertices[index].texCoord0,
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
            var vert = new Vertex
            {
                position = position,
                normal = normal,
                tangent = tangent,
                texCoord0 = texCoord0
            };
            var vert_int = new Stream0
            {
                position = vert.position,
                normal = vert.normal,
                tangent = vert.tangent,
                texCoord0 = vert.texCoord0,
            };
            _vertices[VertexCount] = vert;
            NativeArray<Stream0> buffer = new NativeArray<Stream0>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            buffer[0] = vert_int;
            Mesh.SetVertexBufferData<Stream0>(buffer, 0, VertexCount, 1);
            buffer.Dispose();
            VertexCount++;
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

        }

        public void DeleteFace()
        {

        }

        public void SetFace()
        {

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
            NativeArray<int3> buffer = new NativeArray<int3>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            buffer[0] = int3(vertices[0], vertices[1], vertices[2]);
            buffer[1] = int3(vertices[2], vertices[3], vertices[0]);
            Mesh.SetIndexBufferData<int3>(buffer, 0, TriangleCount, 2);
            buffer.Dispose();
            TriangleCount += 2;
        }

        public void AddTriangle(int3 vertices)
        {
            NativeArray<int3> buffer = new NativeArray<int3>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            buffer[0] = vertices;
            Mesh.SetIndexBufferData<int3>(buffer, 0, TriangleCount, 1);
            buffer.Dispose();
            TriangleCount += 1;
        }

        public void Extrude()
        {

        }

    }

    public struct Vertex
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public float4 tangent; // 16 bytes
        public float2 texCoord0; // 8 bytes
        // maybe add bookkeeping fields?
    }

    public struct Edge
    {

    }

    public struct Face
    {
        public int3 triangle;
    }
}