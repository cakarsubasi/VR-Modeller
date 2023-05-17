using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Mathematics;
using System;

using Meshes;
using Meshes.Test;
using static Unity.Mathematics.math;

public class MeshesTestScript
{

    [Test]
    public void TestEmpty()
    {
        UMesh mesh = Creators.Empty;
        mesh.WriteAllToMesh();
        mesh.CreateVertex();
        mesh.WriteAllToMesh();

        mesh = Creators.Empty;
        mesh.AddFaceUnchecked(Face.Degenerate());
        mesh.WriteAllToMesh();
    }

    /// <summary>
    /// Create a quad from positions
    /// </summary>
    [Test]
    public void TestCreateQuad()
    {
        UMesh mesh = Creators.Empty;
        var vert1 = float3(0f, 0f, 0f);
        var vert2 = float3(1f, 0f, 0f);
        var vert3 = float3(1f, 1f, 0f);
        var vert4 = float3(0f, 1f, 0f);
        mesh.CreateVerticesAndQuad(vert1, vert2, vert3, vert4);
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
            Assert.AreEqual(2, vertex.EdgeCount);
        }
    }

    /// <summary>
    /// Create a quad from dangling vertices
    /// </summary>
    [Test]
    public void TestCreateQuad2()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face = mesh.CreateQuad(new QuadElement<Vertex>(vert1, vert2, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(4, mesh.VertexCount);
        Assert.AreEqual(4, face.VertexCount);
        Assert.AreEqual(4, face.EdgeCount);

        Assert.AreEqual(face.Normal, float3(0f, 0f, 1f));
        foreach (Vertex vertex in mesh.Vertices)
        {
            Assert.AreEqual(float3(0f, 0f, 1f), vertex.Normal);
            Assert.Contains(vertex, face.Vertices);
            Assert.AreEqual(2, vertex.EdgeCount);
        }
    }

    /// <summary>
    /// Create a quad by connecting vertices
    /// </summary>
    [Test]
    public void TestCreateQuad3()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        mesh.AddVertexUnchecked(vert1);

        Vertex vert2 = mesh.CreateVertexConnectedTo(vert1, out _);
        vert2.Position = float3(1f, 0f, 0f);
        Vertex vert3 = mesh.CreateVertexConnectedTo(vert2, out _);
        vert3.Position = float3(1f, 1f, 0f);
        Vertex vert4 = mesh.CreateVertexConnectedTo(vert3, out _);
        vert4.Position = float3(0f, 1f, 0f);

        mesh.CreateQuad(new QuadElement<Vertex>(vert1, vert2, vert3, vert4));
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
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert3, vert4));
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

    /// <summary>
    /// Test creating a 5-gon
    /// </summary>
    [Test]
    public void TestCreateNGon()
    {
        UMesh mesh = Creators.Empty;
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

    /// <summary>
    /// Test creating a 16-gon circle
    /// </summary>
    [Test]
    public void TestCreateCircle()
    {
        UMesh mesh = Creators.Empty;
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

    [Ignore("Tris to quads not implemented yet")]
    /// <summary>
    /// Create a triangle and then add a vertex to it to convert it into a quad
    /// </summary>
    [Test]
    public void TestTriangleToQuad()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        mesh.TrianglesToQuads();
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
    /// Create a square with two triangles, and delete a vertex
    /// Not shared by both triangles.
    /// Only one of the triangles is deleted as a result.
    /// </summary>
    [Test]
    public void TestDeleteOneVertex1()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        mesh.DeleteGeometry(vert2);
        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(3, mesh.VertexCount);

        Assert.AreEqual(2, vert1.Edges.Count);
        Assert.AreEqual(2, vert3.Edges.Count);
        Assert.AreEqual(2, vert4.Edges.Count);
    }

    /// <summary>
    /// Create a square with two triangles, and delete a vertex
    /// Shared by the two triangles.
    /// Both triangles are deleted as a result.
    /// </summary>
    [Test]
    public void TestDeleteOneVertex2()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        mesh.DeleteGeometry(vert1);
        Assert.AreEqual(0, mesh.FaceCount);
        Assert.AreEqual(3, mesh.VertexCount);

        Assert.AreEqual(vert2.Edges.Count, 1);
        Assert.AreEqual(vert3.Edges.Count, 2);
        Assert.AreEqual(vert4.Edges.Count, 1);
    }

    /// <summary>
    /// Create a square with two triangles, and dissolve a vertex
    /// Not shared by both triangles.
    /// The result is identical to the deletion case.
    /// </summary>
    [Test]
    public void TestDissolveOneVertex1()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        mesh.DissolveVertex(vert2);
        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(3, mesh.VertexCount);

        Assert.AreEqual(vert1.Edges.Count, 2);
        Assert.AreEqual(vert3.Edges.Count, 2);
        Assert.AreEqual(vert4.Edges.Count, 2);
    }

    /// <summary>
    /// Create a square with two triangles, and dissolve a vertex
    /// Shared by the two triangles.
    /// A new face that is different than the previous two triangles is left.
    /// </summary>
    [Test]
    public void TestDissolveOneVertex2()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert2, vert3));
        Face face2 = mesh.CreateTriangle(new TriangleElement<Vertex>(vert1, vert3, vert4));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        mesh.DissolveVertex(vert1);
        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(3, mesh.VertexCount);

        Assert.AreEqual(vert2.Edges.Count, 2);
        Assert.AreEqual(vert3.Edges.Count, 2);
        Assert.AreEqual(vert4.Edges.Count, 2);
    }

    [Test]
    public void TestDeleteOneFace()
    {
        UMesh mesh = Creators.Empty;
        Vertex vert1 = Vertex.Dangling(float3(0f, 0f, 0f));
        Vertex vert2 = Vertex.Dangling(float3(1f, 0f, 0f));
        Vertex vert3 = Vertex.Dangling(float3(1f, 1f, 0f));
        Vertex vert4 = Vertex.Dangling(float3(0f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert1, vert2, vert3, vert4);
        Face face1 = mesh.CreateQuad(new QuadElement<Vertex>(vert1, vert2, vert3, vert4));
        Vertex vert5 = Vertex.Dangling(float3(2f, 0f, 0f));
        Vertex vert6 = Vertex.Dangling(float3(2f, 1f, 0f));
        mesh.AddVerticesUnchecked(vert5, vert6);
        Face face2 = mesh.CreateQuad(new QuadElement<Vertex>(vert2, vert5, vert6, vert3));
        mesh.OptimizeIndices();
        mesh.RecalculateNormals();

        mesh.DeleteGeometry(face1);

        Assert.AreEqual(1, mesh.FaceCount);
        Assert.AreEqual(6, mesh.VertexCount);
        Assert.IsTrue(vert1.Alive);
        Assert.IsTrue(vert2.Alive);
        Assert.IsTrue(vert3.Alive);
        Assert.IsTrue(vert4.Alive);
        Assert.IsTrue(vert5.Alive);
        Assert.IsTrue(vert6.Alive);
        Assert.IsFalse(vert1.IsConnected(face1));
        Assert.IsFalse(vert2.IsConnected(face1));
        Assert.IsFalse(vert3.IsConnected(face1));
        Assert.IsFalse(vert4.IsConnected(face1));
        Assert.AreEqual(2, vert5.EdgeCount);
        Assert.AreEqual(2, vert6.EdgeCount);
        Assert.AreEqual(1, vert5.FaceCount);
        Assert.AreEqual(1, vert6.FaceCount);
    }

    [Ignore("Merge not implemented yet")]
    [Test]
    public void TestMergeByDistance()
    {
        throw new NotImplementedException { };
    }

    [Test]
    public void TestMergeTwoVertices()
    {
        UMesh mesh = Creators.Cube;

        mesh.MergeVertices(mesh.FindByPosition(float3(-1f, -1f, -1f)),
            mesh.FindByPosition(float3(-1f, -1f, 1f)));

        Assert.AreEqual(6, mesh.FaceCount);
        Assert.AreEqual(11, mesh.EdgeCount);
        Assert.AreEqual(7, mesh.VertexCount);
    }

    [Test]
    public void TestDeepCopy()
    {
        UMesh mesh1 = Creators.Quad;
        mesh1.WriteAllToMesh();

        UMesh mesh2 = mesh1.DeepCopy();
        mesh2.WriteAllToMesh();

        Assert.AreEqual(mesh1.VertexCount, mesh2.VertexCount);
        Assert.AreEqual(mesh1.EdgeCount, mesh2.EdgeCount);
        Assert.AreEqual(mesh1.FaceCount, mesh2.FaceCount);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    //[UnityTest]
    //public IEnumerator NewTestScriptWithEnumeratorPasses()
    //{
    //    // Use the Assert class to test conditions.
    //    // Use yield to skip a frame.
    //    yield return null;
    //}
}
