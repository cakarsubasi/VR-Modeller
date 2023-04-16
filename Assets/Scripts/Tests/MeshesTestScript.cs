using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

    // create a simple quad
    [Test]
    public void TestCreateQuad()
    {
        EditableMeshImpl mesh = EmptyEditableMesh();
        var vert1 = float3(0f, 0f, 0f);
        var vert2 = float3(1f, 0f, 0f);
        var vert3 = float3(1f, 1f, 0f);
        var vert4 = float3(0f, 1f, 0f);
        mesh.AddFace(vert1, vert2, vert3, vert4);
        
    }

    [Test]
    public void TestCreateQuad2()
    {

    }

    /// <summary>
    /// Create two triangles and then merge them together
    /// </summary>
    [Test]
    public void TestCreateTwoTris()
    {

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
