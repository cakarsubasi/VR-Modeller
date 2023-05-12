using UnityEngine;
using Meshes;
using System;
#nullable enable

using static Unity.Mathematics.math;

[Obsolete("This is a test component for the CopySetup method in UMesh." +
    " It should not be used externally.")]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CopySetupTestComponent : MonoBehaviour
{
    UMesh meshInternal;

    [SerializeField]
    Mesh? CopyFromMesh;

    public UMesh MeshInternal { get => meshInternal; private set => meshInternal = value; }

    private void OnEnable()
    {

        //meshInternal = default;

        if (CopyFromMesh != null)
        {
            meshInternal = UMesh.Create();
            Debug.Log($"Vertex count: {CopyFromMesh.vertexCount}");
            GetComponent<MeshFilter>().mesh = meshInternal.CopySetup(CopyFromMesh);
            
        }
        else
        {
            meshInternal = UMesh.Create(new Mesh(), "Flat quad");
            var vert1 = float3(0f, 0f, 0f);
            var vert2 = float3(1f, 0f, 0f);
            var vert3 = float3(1f, 1f, 0f);
            var vert4 = float3(0f, 1f, 0f);
            meshInternal.CreateVerticesAndQuad(vert1, vert2, vert3, vert4);
            GetComponent<MeshFilter>().mesh = meshInternal.Mesh;
        }
        meshInternal.OptimizeRendering();
        meshInternal.WriteAllToMesh();
        Debug.Log($"{meshInternal}");
        
    }
    
    private void Update()
    {
        meshInternal.WriteAllToMesh();
    }

}