using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

using Meshes;
using static Unity.Mathematics.math;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExtrusionMergeTestComponent : MonoBehaviour
{
    UMesh meshInternal;

    private void OnEnable()
    {
        meshInternal = UMesh.Create();

        Vertex vertex1 = meshInternal.CreateVertex(float3(-1f, -1f, -1f));

        Vertex vertex2 = meshInternal.CreateVertexConnectedTo(vertex1, out Edge edge1);
        vertex2.Position = float3(1f, -1f, -1f);
        // . - .

        //vertex1.Selected = true;

        meshInternal.Extrude(edge1);
        edge1.MoveRelative(float3(0f, 2f, 0f));
        // . - .
        // |   |
        // . - .

        List<Vertex> all = new(meshInternal.Vertices);
        meshInternal.Extrude(all);
        meshInternal.MoveSelectionRelative(all, float3(0f, 0f, 2f));
        meshInternal.FlipNormals();

        Vertex vert1 = meshInternal.FindByPosition(float3(-1f, -1f, -1f));
        Vertex vert2 = meshInternal.FindByPosition(float3(1f, -1f, -1f));
        Edge edge2 = vert1.GetEdgeTo(vert2);
        meshInternal.Extrude(edge2);
        edge2.MoveRelative(float3(0f, 2f, 0f));
        meshInternal.MergeByDistance();

        //vert1.Selected = true;
        //vert2.Selected = true;

        vert1 = meshInternal.FindByPosition(float3(-1f, -1f, -1f));
        vert2 = meshInternal.FindByPosition(float3(1f, -1f, -1f));
        edge2 = vert1.GetEdgeTo(vert2);
        edge2.MoveRelative(float3(0f, -1f, -1f));

        meshInternal.OptimizeIndices();
        meshInternal.RecalculateNormals();
        meshInternal.RecalculateTangents();

        vert1.Selected = true;
        vert2.Selected = true;

        //meshInternal.FindByPosition(float3(-1f, 1f, -1f)).Selected = true;

        meshInternal.WriteAllToMesh();

        GetComponent<MeshFilter>().mesh = meshInternal.Mesh;
    }


}
