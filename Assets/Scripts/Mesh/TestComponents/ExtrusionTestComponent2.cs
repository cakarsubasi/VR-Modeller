using System.Collections;
using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;
using Meshes;

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ExtrusionTestComponent2 : MonoBehaviour
{
    private void OnEnable()
    {
        UMesh mesh = CubeDeletionTestComponent.CreateCube();
        GetComponent<MeshFilter>().mesh = mesh.Mesh;

        List<Vertex> verts = new(); // = mesh.Faces.First().Vertices;
        float3 pos0 = float3(-1f, -1f, -1f);
        float3 pos1 = float3(-1f, -1f, 1f);
        float3 pos2 = float3(-1f, 1f, -1f);
        float3 pos3 = float3(-1f, 1f, 1f);

        Vertex ex0 = mesh.FindByPosition(pos0);
        Vertex ex1 = mesh.FindByPosition(pos1);
        Vertex ex2 = mesh.FindByPosition(pos2);
        Vertex ex3 = mesh.FindByPosition(pos3);
        verts.Add(ex0);
        verts.Add(ex1);
        verts.Add(ex2);
        verts.Add(ex3);

        mesh.Extrude(verts);
        mesh.MoveSelectionRelative(verts, float3(-1f, 0f, 0f));

        mesh.DeleteGeometry(verts);
        verts.Clear();


        Vertex ex0new = mesh.FindByPosition(pos0);
        Vertex ex1new = mesh.FindByPosition(pos1);
        Vertex ex2new = mesh.FindByPosition(pos2);
        Vertex ex3new = mesh.FindByPosition(pos3);
        verts.Add(ex0new);
        verts.Add(ex1new);
        verts.Add(ex2new);
        verts.Add(ex3new);
        mesh.Extrude(verts);
        mesh.MoveSelectionRelative(verts, float3(-1f, 0f, 0f));

        mesh.RecalculateAllAndWriteToMesh();

    }

}