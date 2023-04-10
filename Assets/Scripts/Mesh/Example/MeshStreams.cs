using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Jobs;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Meshes
{

    public interface IMeshStreams
    {
        // TODO: need to make vertexcount and indexcount configurable
        void Setup(Mesh.MeshData data, int vertexCount, int indexCount);
        void SetVertex(int index, VertexLegacy data);
        void SetTriangle(int index, int3 triangle);


    }

    public struct MeshStream : IMeshStreams
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct Stream0
        {
            public float3 position; // 12 bytes
            public float3 normal; // 12 bytes
            public float4 tangent; // 16 bytes
            public float2 texCoord0; // 8 bytes
        }

        private static readonly int vertexAttributeCount = 4;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<Stream0> stream0;

        [NativeDisableContainerSafetyRestriction]
        NativeArray<int3> triangles;

        int VertexCount { get; }
        int IndexCount { get; }

        public void Setup(Mesh.MeshData meshData, int vertexCount, int indexCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
                vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // positions
            vertexAttributes[0] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Position,
                dimension: 3,
                stream: 0);
            // normals
            vertexAttributes[1] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Normal,
                dimension: 3,
                stream: 0);
            // tangents
            vertexAttributes[2] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Tangent,
                format: VertexAttributeFormat.Float32,
                dimension: 4,
                stream: 0);
            // uv coordinates
            vertexAttributes[3] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.TexCoord0,
                format: VertexAttributeFormat.Float32,
                dimension: 2,
                stream: 0);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            vertexAttributes.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount),
                MeshUpdateFlags.DontRecalculateBounds |MeshUpdateFlags.DontValidateIndices);

            stream0 = meshData.GetVertexData<Stream0>();
            triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);
        }

        public void Resize(Mesh.MeshData meshData, int vertexCount, int indexCount)
        {
            var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
    vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            // positions
            vertexAttributes[0] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Position,
                dimension: 3,
                stream: 0);
            // normals
            vertexAttributes[1] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Normal,
                dimension: 3,
                stream: 0);
            // tangents
            vertexAttributes[2] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.Tangent,
                format: VertexAttributeFormat.Float32,
                dimension: 4,
                stream: 0);
            // uv coordinates
            vertexAttributes[3] = new VertexAttributeDescriptor(
                attribute: VertexAttribute.TexCoord0,
                format: VertexAttributeFormat.Float32,
                dimension: 2,
                stream: 0);

            meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
            vertexAttributes.Dispose();

            meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount),
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);

            var new_stream0 = meshData.GetVertexData<Stream0>();
            var new_triangles = meshData.GetIndexData<int>().Reinterpret<int3>(4);

            for (int i = 0; i < VertexCount; ++i)
            {
                new_stream0[i] = stream0[i];
            }
            for (int i = 0; i < IndexCount; ++i)
            {
                new_triangles[i] = triangles[i];
            }
            stream0.Dispose();
            triangles.Dispose();
            stream0 = new_stream0;
            triangles = new_triangles;
        }

        public void MoveVertex(int index, float3 position)
        {
            var copy = stream0[index];
            copy.position = position;
            // TODO: correct normals and tangents?
            stream0[index] = copy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetVertex(int index, VertexLegacy vertex) => stream0[index] = new Stream0
        {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0,
        };

        public void SetTriangle(int index, int3 triangle) => triangles[index] = triangle;
    }

}