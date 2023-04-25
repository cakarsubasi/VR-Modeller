using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class VertexController : MonoBehaviour
{
    MeshController meshController;
    int vertexIndex;
    bool isSelected;
    GameObject targetMesh;
    MeshCollider parentCollider;

    public int VertexIndex { get => vertexIndex; set => vertexIndex = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }

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

            meshController.editableMesh.Vertices[VertexIndex].Position = (float3)targetMesh.transform.InverseTransformPoint(transform.position);
            meshController.editableMesh.WriteAllToMesh();

            if (parentCollider != null)
            {
                parentCollider.sharedMesh = meshController.Mesh;
            }
        }
        //transform.localScale = parent.transform.lossyScale / (Mathf.Pow(parent.transform.lossyScale.x, 2) * 20);
    }
}
