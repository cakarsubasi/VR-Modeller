using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VertexController : MonoBehaviour
{
    Material material;
    MeshController meshController;
    int[] assignedVertices;

    public int[] AssignedVertices { get => assignedVertices; set => assignedVertices = value; }

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
        meshController = transform.parent.GetComponent<MeshController>();
    }

    private void OnMouseEnter()
    {
        material.SetColor("_Color", Color.red);
    }

    private void OnMouseExit()
    {
        material.SetColor("_Color", Color.yellow);
    }

    private void OnMouseDrag()
    {
        material.SetColor("_Color", Color.blue);

        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Camera.main.transform.localPosition.y));

        transform.position = worldPos;

        for (int i = 0; i < AssignedVertices.Length; i++)
        {
            meshController.Vertices[AssignedVertices[i]] = transform.localPosition;
        }

        meshController.Mesh.vertices = meshController.Vertices.ToArray();

    }
}
