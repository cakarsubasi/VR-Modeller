using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;

using Meshes;
using static Unity.Mathematics.math;

public class MeshesTestScript
{

    public EditableMeshImpl EmptyEditableMesh()
    {
        EditableMeshImpl editableMesh = default;
        editableMesh.Setup(new Mesh());
        return editableMesh;
    }

    /// <summary>
    /// Create a quad from positions
    /// </summary>
    [Test]
    public void TestCreateQuad()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        var vert1 = float3(0f, 0f, 0f);
        var vert2 = float3(1f, 0f, 0f);
        var vert3 = float3(1f, 1f, 0f);
        var vert4 = float3(0f, 1f, 0f);
        mesh.CreateVerticesAndQuad(vert1, vert2, vert3, vert4);
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(mesh.FaceCount, 1);
        Assert.AreEqual(mesh.VertexCount, 4);

        Face face = mesh.Faces[0];
        Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(vertex.Normal, float3(0f, 0f, 1f));
            Assert.Contains(vertex, face.Vertices);
        }
    }

    /// <summary>
    /// Create a quad from dangling vertices
    /// </summary>
    [Test]
    public void TestCreateQuad2()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        mesh.CreateQuad(new QuadVerts(vert1, vert2, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(4, mesh.VertexCount);

        Face face = mesh.Faces[0];
        Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(float3(0f, 0f, 1f), vertex.Normal);
            Assert.Contains(vertex, face.Vertices);
        }
    }

    /// <summary>
    /// Create a quad by connecting vertices
    /// </summary>
    [Test]
    public void TestCreateQuad3()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        mesh.AddVertexUnchecked(vert1);

        Vertex vert2 = mesh.CreateVertexConnectedTo(vert1);
        vert2.Position = float3(1f, 0f, 0f);
        Vertex vert3 = mesh.CreateVertexConnectedTo(vert2);
        vert3.Position = float3(1f, 1f, 0f);
        Vertex vert4 = mesh.CreateVertexConnectedTo(vert3);
        vert4.Position = float3(0f, 1f, 0f);

        mesh.CreateQuad(new QuadVerts(vert1, vert2, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(4, mesh.VertexCount);

        Face face = mesh.Faces[0];
        Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(float3(0f, 0f, 1f), vertex.Normal);
            Assert.Contains(vertex, face.Vertices);
        }
    }

    /// <summary>
    /// Create two triangles
    /// </summary>
    [Test]
    public void TestCreateTwoTris()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleVerts(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleVerts(vert1, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(2, mesh.FaceCount);
        Assert.AreEqual(4, mesh.VertexCount);

        foreach (Face face in mesh.Faces)
        {
            Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        }

        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(float3(0f, 0f, 1f), vertex.Normal);
        }
    }

    [Test]
    public void TestCreateNGon()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        Vertex vert5 = Vertex.Dangling(float3(-0.5f, -0.5f, 0f));
        Vertex[] vertices = mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4, vert5);
        Face face = mesh.CreateNGon(vertices);
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(5, mesh.VertexCount);

        Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(float3(0f, 0f, 1f), vertex.Normal);
            Assert.Contains(vertex, face.Vertices);
        }

    }

    [Test]
    public void TestCreateCircle()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        int points = 16;
        List<Vertex> vertices = new(16);
        for (int i = 0; i < points; ++i)
        {
            float angle = ((float)i / 16.0f);
            float x = math.cos(angle);
            float y = math.sin(angle);
            float3 position = float3(x, y, 0f);
            vertices.Add(mesh.CreateVertex(position));
        }
        Face face = mesh.CreateNGon(vertices);
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(16, mesh.VertexCount);

        Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(float3(0f, 0f, 1f), vertex.Normal);
            Assert.Contains(vertex, face.Vertices);
        }
    }

    /// <summary>
    /// Create a triangle and then add a vertex to it to convert it into a quad
    /// </summary>
    [Test]
    public void TestTriangleToQuad()
    {

    }

    [Test]
    public void TestTwoQuadsSideBySide()
    {

    }

    [Test]
    public void TestCreateQuadFromTwoTris()
    {

    }

    [Test]
    public void TestExtrudeOneVertex()
    {

    }

    [Test]
    public void TestExtrudeOneEdge()
    {

    }

    /// <summary>
    /// Extrude one face from a quad to create a cube.
    /// <br>There is some subtletly involved in this test as the 
    /// normals have to face the same direction across the mesh when the extrusion is done
    /// </br>
    /// </summary>
    [Test]
    public void TestExtrudeOneFace()
    {

    }

    [Test]
    public void TestDeleteOneVertex()
    {

    }

    [Test]
    public void TestDissolveOneVertex()
    {

    }

    [Test]
    public void TestDeleteOneFace()
    {

    }


    // A Test behaves as an ordinary method
    [Test]
    public void NewTestScriptSimplePasses()
    {
        // Use the Assert class to test conditions
        
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator NewTestScriptWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
