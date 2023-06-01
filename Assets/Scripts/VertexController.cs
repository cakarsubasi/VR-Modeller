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
            Vertex.Position = (float3)targetMesh.transform.InverseTransformPoint(transform.position);
            meshController.EditableMesh.WriteAllToMesh();

            if (parentCollider != null)
            {
                parentCollider.sharedMesh = meshController.EditableMesh.Mesh;
            }
        }
        else
        {
            transform.localScale = Vector3.Scale(Vector3.one / 15, new Vector3(1f / gameObject.transform.parent.parent.localScale.x, 1f / gameObject.transform.parent.parent.localScale.y, 1f / gameObject.transform.parent.parent.localScale.z));
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

        ObjectController.Instance.UpdateButtonInteractability();
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
