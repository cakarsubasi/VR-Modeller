using Meshes;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VertexController : MonoBehaviour
{
    public Material normalMaterial, activeMaterial, selectedMaterial;

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
            meshController.EditableMesh.WriteAllToMesh();

            if (parentCollider != null)
            {
                parentCollider.sharedMesh = meshController.EditableMesh.Mesh;
            }
        }
        else
        {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float minValue = 0.1f;
            float maxValue = 0.25f;
            float scaleFactor = 0.05f;
            transform.localScale = Vector3.one * Mathf.Clamp(distance * scaleFactor, minValue, maxValue);
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
        //meshController.CreateObjectInActiatedVertices();
    }

    public void SetSelectState()
    {
        if (IsSelected)
        {
            GetComponent<MeshRenderer>().material = selectedMaterial;
        }
        else
        {
            if (isActivated)
            {
                GetComponent<MeshRenderer>().material = activeMaterial;
            }
            else
            {
                GetComponent<MeshRenderer>().material = normalMaterial;
            }
        }
    }
}
