using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Meshes;

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditableMesh : MonoBehaviour
{
    EditableMeshImpl meshInternal;
    private void OnEnable()
    {
        var mesh = new Mesh
        {
            name = "Editable Mesh"
        };
        meshInternal = default;
        meshInternal.Setup(mesh);


        GetComponent<MeshFilter>().mesh = mesh;

        var vert1 = float3(0f, 0f, 0f);
        var vert2 = float3(1f, 0f, 0f);
        var vert3 = float3(1f, 1f, 0f);
        var vert4 = float3(0f, 1f, 0f);
        meshInternal.AddFace(vert1, vert2, vert3, vert4);
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

        public int AddVertex(float3 position)
        {
            var vert = new Vertex
            {
                position = position,
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

        public void AddFace(float3 pos1, float3 pos2, float3 pos3, float3 pos4)
        {
            var indices = new int[4];
            indices[0] = AddVertex(pos1);
            indices[1] = AddVertex(pos2);
            indices[2] = AddVertex(pos3);
            indices[3] = AddVertex(pos4);
            AddFace(indices);
        }

        public void AddFace(int[] vertices)
        {
            NativeArray<int3> buffer = new NativeArray<int3>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            buffer[0] = int3(vertices[0], vertices[1], vertices[2]);
            buffer[1] = int3(vertices[2], vertices[3], vertices[0]);
            Mesh.SetIndexBufferData<int3>(buffer, 0, TriangleCount, 2);
            buffer.Dispose();
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