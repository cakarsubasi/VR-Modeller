using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

using Meshes;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeTestComponent : MonoBehaviour
{

    UMesh meshInternal;

    private void OnEnable()
    {
        meshInternal = CreateCylinder();
        meshInternal.RecalculateNormals();
        meshInternal.WriteAllToMesh();

        GetComponent<MeshFilter>().mesh = meshInternal.Mesh;
    }

    public static UMesh CreateCube()
    {
        UMesh mesh = UMesh.Create();

        Vertex v1 = mesh.CreateVertex(float3(-1f, -1f, -1f));
        Vertex v2 = mesh.CreateVertex(float3(1f, -1f, -1f));
        Vertex v3 = mesh.CreateVertex(float3(1f, -1f, 1f));
        Vertex v4 = mesh.CreateVertex(float3(-1f, -1f, 1f));

        Vertex v5 = mesh.CreateVertex(float3(-1f, 1f, -1f));
        Vertex v6 = mesh.CreateVertex(float3(1f, 1f, -1f));
        Vertex v7 = mesh.CreateVertex(float3(1f, 1f, 1f));
        Vertex v8 = mesh.CreateVertex(float3(-1f, 1f, 1f));

        mesh.CreateQuad(new QuadElement<Vertex>(v1, v2, v3, v4));
        mesh.CreateQuad(new QuadElement<Vertex>(v8, v7, v6, v5));

        mesh.CreateQuad(new QuadElement<Vertex>(v1, v4, v8, v5));
        mesh.CreateQuad(new QuadElement<Vertex>(v1, v5, v6, v2));

        mesh.CreateQuad(new QuadElement<Vertex>(v2, v6, v7, v3));
        mesh.CreateQuad(new QuadElement<Vertex>(v3, v7, v8, v4));

        return mesh;
    }

    public static UMesh CreateCylinder(int points = 16, float h = 2f, float r = 1f)
    {
        UMesh mesh = UMesh.Create();

        List<Vertex> verticesTop = new(points);
        for (int i = 0; i < points; ++i)
        {
            float angle = ((float)i / (float) points) * PI * 2f;
            float x = math.cos(angle) * r;
            float y = math.sin(angle) * r;
            float3 position = float3(x, h / 2f, y);
            verticesTop.Add(mesh.CreateVertex(position));
        }
        Face faceTop = mesh.CreateFace((ICollection<Vertex>) verticesTop);

        List<Vertex> verticesBottom = new(points);
        for (int i = 0; i < points; ++i)
        {
            float angle = ((float)i / (float) points) * PI * 2f;
            float x = math.cos(angle) * r;
            float y = math.sin(angle) * r;
            float3 position = float3(x, - h / 2f, y);
            verticesBottom.Add(mesh.CreateVertex(position));
        }
        Face faceBottom = mesh.CreateNGon(verticesBottom);

        for (int i = 0; i < points; ++i)
        {
            Vertex vert1 = verticesTop[i];
            Vertex vert3 = verticesTop[(i + 1) % points];
            Vertex vert4 = verticesBottom[(i + 1) % points];
            Vertex vert2 = verticesBottom[i];
            mesh.CreateQuad(new QuadElement<Vertex>(vert1, vert2, vert3, vert4));
        }

        return mesh;
    }
}
