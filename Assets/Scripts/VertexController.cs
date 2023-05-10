using Meshes;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VertexController : MonoBehaviour
{
    public Material normalMaterial, activeMaterial;

    MeshController meshController;
    bool isSelected, isActivated = false;
    GameObject targetMesh;
    MeshCollider parentCollider;
    Vertex vertex;

    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public bool IsActivated { get => isActivated; set => isActivated = value; }
    public Vertex Vertex { get => vertex; set => vertex = value; }

    private void Start()
    {
        targetMesh = transform.parent.parent.gameObject;
        meshController = targetMesh.GetComponent<MeshController>();
        parentCollider = targetMesh.GetComponent<MeshCollider>();
    }



    private void Update()
    {
        if (IsSelected)
        {
            //meshController.Vertices[VertexIndex] = targetMesh.transform.InverseTransformPoint(transform.position);
            //meshController.Mesh.vertices = meshController.Vertices.ToArray();

            Vertex.Position = (float3)targetMesh.transform.InverseTransformPoint(transform.position);
            meshController.editableMesh.WriteAllToMesh();

            if (parentCollider != null)
            {
                parentCollider.sharedMesh = meshController.editableMesh.Mesh;
            }
        }
    }

    public void SetActiveState()
    {
        if (IsSelected) return;

        IsActivated = !IsActivated;

        if (IsActivated)
        {
            GetComponent<MeshRenderer>().material = activeMaterial;
            meshController.ActiveVertices.Add(gameObject);
        }
        else
        {
            GetComponent<MeshRenderer>().material = normalMaterial;
            meshController.ActiveVertices.Remove(gameObject);
        }
    }
}
