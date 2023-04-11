using UnityEngine;
using Meshes;
#nullable enable

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EditableMesh : MonoBehaviour
{
    EditableMeshImpl meshInternal;

    [SerializeField]
    Mesh? CopyFromMesh;

    public EditableMeshImpl MeshInternal { get => meshInternal; private set => meshInternal = value; }

    private void OnEnable()
    {

        //meshInternal = default;

        if (CopyFromMesh != null)
        {
            meshInternal = default;
            Debug.Log($"Vertex count: {CopyFromMesh.vertexCount}");
            GetComponent<MeshFilter>().mesh = meshInternal.CopySetup(CopyFromMesh);
            
        }
        else
        {
            var mesh = new Mesh
            {
                name = "Flat quad"
            };
            meshInternal.Setup(mesh);
            var vert1 = float3(0f, 0f, 0f);
            var vert2 = float3(1f, 0f, 0f);
            var vert3 = float3(1f, 1f, 0f);
            var vert4 = float3(0f, 1f, 0f);
            meshInternal.AddFace(vert1, vert2, vert3, vert4);
            GetComponent<MeshFilter>().mesh = mesh;
        }
        meshInternal.WriteAllToMesh();
        Debug.Log($"{meshInternal}");
        
    }

    private void Update()
    {
        meshInternal.WriteAllToMesh();
    }

}