using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using System.Runtime.InteropServices;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedSingleStreamMesh : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public float3 position; // 12 bytes
        public float3 normal; // 12 bytes
        public half4 tangent; // 8 bytes
        public half2 texCoord0; // 4 bytes
    }

    private void OnEnable()
    {
        // Vertices have 4 streams: positions, normals, tangents, uvs
        int vertexAttributeCount = 4;
        // may consider having "controlled garbage" out of bounds values to avoid having to reallocate
        int vertexCount = 4;
        int triangleIndexCount = 6;

        // allocate a single mesh
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );

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
            format: VertexAttributeFormat.Float16,
            dimension: 4,
            stream: 0);
        // uv coordinates
        vertexAttributes[3] = new VertexAttributeDescriptor(
            attribute: VertexAttribute.TexCoord0,
            format: VertexAttributeFormat.Float16,
            dimension: 2,
            stream: 0);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();

        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

        half h0 = half(0f), h1 = half(1f);

        var vertex = new Vertex
        {
            normal = back(),
            tangent = half4(h1, h0, h0, half(-1f)),
        };

        vertex.position = 0f;
        vertex.texCoord0 = h0;
        vertices[0] = vertex;

        vertex.position = right();
        vertex.texCoord0 = half2(h1, h0);
        vertices[1] = vertex;

        vertex.position = up();
        vertex.texCoord0 = half2(h0, h1);
        vertices[2] = vertex;

        vertex.position = float3(1f, 1f, 0f);
        vertex.texCoord0 = h1;
        vertices[3] = vertex;


        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt32);
        // set triangles
        NativeArray<uint> triangleIndices = meshData.GetIndexData<uint>();
        triangleIndices[0] = 0;
        triangleIndices[1] = 2;
        triangleIndices[2] = 1;
        triangleIndices[3] = 1;
        triangleIndices[4] = 2;
        triangleIndices[5] = 3;

        // set submeshes
        var bounds = new Bounds(new Vector3(0.5f, 0.5f), new Vector3(1f, 1f));

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(
            index: 0,
            desc: new SubMeshDescriptor(0, triangleIndexCount)
            {
                bounds = bounds,
                vertexCount = vertexCount,
            },
            flags: MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Multi-stream Mesh"
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }


}