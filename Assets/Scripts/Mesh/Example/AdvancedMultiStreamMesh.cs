using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AdvancedMultiStreamMesh : MonoBehaviour
{

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
            stream: 1);
        // tangents
        vertexAttributes[2] = new VertexAttributeDescriptor(
            attribute: VertexAttribute.Tangent,
            format: VertexAttributeFormat.Float16,
            dimension: 4,
            stream: 2);
        // uv coordinates
        vertexAttributes[3] = new VertexAttributeDescriptor(
            attribute: VertexAttribute.TexCoord0,
            format: VertexAttributeFormat.Float16,
            dimension: 2,
            stream: 3);

        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        meshData.SetIndexBufferParams(triangleIndexCount, IndexFormat.UInt32);
        vertexAttributes.Dispose();

        // set positions
        NativeArray<float3> positions = meshData.GetVertexData<float3>(stream: 0);
        positions[0] = 0f;
        positions[1] = right();
        positions[2] = up();
        positions[3] = float3(1f, 1f, 0f);

        // set normals
        NativeArray<float3> normals = meshData.GetVertexData<float3>(stream: 1);
        normals[0] = normals[1] = normals[2] = normals[3] = back();

        // set tangents
        NativeArray<half4> tangents = meshData.GetVertexData<half4>(stream: 2);
        tangents[0] = tangents[1] = tangents[2] = tangents[3] = half4(half(1f), half(0f), half(0f), half(-1f));

        // set uvs
        NativeArray<half2> texCoords = meshData.GetVertexData<half2>(3);
        texCoords[0] = half(0f);
        texCoords[1] = half2(half(1f), half(0f));
        texCoords[2] = half2(half(0f), half(1f));
        texCoords[3] = half(1f);

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
            flags : MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Multi-stream Mesh"
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
    }

}
