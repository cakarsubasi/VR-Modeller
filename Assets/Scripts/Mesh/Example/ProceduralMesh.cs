using System.Collections;
using UnityEngine;
using Meshes;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour
{

    Mesh mesh;

    static MeshJobScheduleDelegate[] jobs =
    {
        MeshJob<EmptyMesh, MeshStream>.ScheduleParallel,
        MeshJob<QuadMesh, MeshStream>.ScheduleParallel,
    };

    public enum MeshType
    {
        Empty, Quad
    }

    [SerializeField]
    MeshType meshType;

    private void Awake()
    {
        mesh = new Mesh
        {
            name = "Procedural Mesh"
        };
        GenerateMesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void GenerateMesh()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        jobs[(int)meshType](mesh, meshData, default).Complete();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
    }

    
}
