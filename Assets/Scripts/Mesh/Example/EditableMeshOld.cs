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
public class EditableMeshOld : MonoBehaviour
{
    Mesh mesh;
    MeshStream streams;

    private void Awake()
    {
        mesh = new Mesh
        {
            name = "Procedural Mesh"
        };
        GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;

        var vertex = new Vertex
        {
            position = float3(-1f, 0f, 0f)
        };
        vertex.normal.z = -1f;
        vertex.tangent.xw = float2(1f, -1f);
        //streams.SetVertex(0, vertex);
        var single_vert = new MeshStream.Stream0
        {
            position = vertex.position,
            normal = vertex.normal,
            tangent = vertex.tangent,
            texCoord0 = vertex.texCoord0,
        };
        NativeArray<MeshStream.Stream0> buffer = new NativeArray<MeshStream.Stream0>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        buffer[0] = single_vert;
        mesh.SetVertexBufferData<MeshStream.Stream0>(buffer, 0, 0, 1);
        buffer.Dispose();
    }

    private void GenerateMesh()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        streams = default;
        streams.Setup(meshData, 4, 6);

        var vertex = new Vertex();
        vertex.normal.z = -1f;
        vertex.tangent.xw = float2(1f, -1f);
        streams.SetVertex(0, vertex);

        vertex.position = right();
        vertex.texCoord0 = float2(1f, 0f);
        streams.SetVertex(1, vertex);

        vertex.position = up();
        vertex.texCoord0 = float2(0f, 1f);
        streams.SetVertex(2, vertex);

        vertex.position = float3(1f, 1f, 0f);
        vertex.texCoord0 = 1f;
        streams.SetVertex(3, vertex);

        streams.SetTriangle(0, int3(0, 2, 1));
        streams.SetTriangle(1, int3(1, 2, 3));

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }

    private void EditMesh()
    {

    }

    public Vector3[] GetVertices()
    {
        return new Vector3[0];
    }
}