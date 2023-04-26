using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

using Meshes;
using static Unity.Mathematics.math;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExtrusionTestComponent : MonoBehaviour
{
    UMesh meshInternal;

    private void OnEnable()
    {
        var unityMesh = new Mesh
        {
            name = "Extrusion test"
        };
        meshInternal.Setup(unityMesh);

        Vertex vertex1 = meshInternal.CreateVertex(float3(-1f, 1f, 0f));

        Vertex vertex2 = meshInternal.CreateVertexConnectedTo(vertex1, out Edge edge1);
        vertex2.Position = float3(0f, 1f, 0f);
        // . - .
        Vertex vertex3 = meshInternal.CreateVertexConnectedTo(vertex2, out Edge edge2);
        vertex3.Position = float3(1f, 1f, 0f);
        // . - . - .

        List<Edge> edges = new() { edge1, edge2 };
        meshInternal.Extrude(edges);
        meshInternal.MoveSelectionRelative(edges, float3(0f, -1f, 0f));
        // . - . - .
        // |   |   |
        // . - . - .

        meshInternal.Extrude(edges);
        meshInternal.MoveSelectionRelative(edges, float3(0f, -1f, 0f));
        // . - . - .
        // |   |   |
        // . - . - .
        // |   |   |
        // . - . - .

        List<Vertex> allVertices = new(meshInternal.Vertices);
        meshInternal.Extrude(allVertices);
        meshInternal.MoveSelectionRelative(allVertices, float3(0f, 0f, 1f));

        meshInternal.OptimizeIndices();
        meshInternal.RecalculateNormals();

        meshInternal.WriteAllToMesh();

        GetComponent<MeshFilter>().mesh = unityMesh;

    }


}
