using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshController : MonoBehaviour
{
    public GameObject vertexHighlight;
    public InputActionProperty closeAction, openAction;

    Mesh mesh;
    MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();
    List<GameObject> vertexObjects = new List<GameObject>();
    bool isSelected;

    public List<Vector3> Vertices { get => vertices; set => vertices = value; }
    public Mesh Mesh { get => mesh; set => mesh = value; }

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        Vertices = mesh.vertices.ToList();

        //Debug.Log("Vertices: " + Vertices.Count);

        List<Vector3> updatedVertices = new List<Vector3>();

        for (var i = 0; i < Vertices.Count; i++)
        {
            List<Vector3> sameVertices = Vertices.FindAll(x => x == Vertices[i]);

            if (!updatedVertices.Contains(sameVertices[0]))
            {
                updatedVertices.Add(sameVertices[0]);

                List<int> vertexIndexes = new List<int>();
                for (int k = 0; k < Vertices.Count; k++)
                {
                    if (Vertices[k] == sameVertices[0])
                    {
                        vertexIndexes.Add(k);
                        //Debug.Log(sameVertices[0] + " " + k);
                    }
                }
                GameObject vertex = Instantiate(vertexHighlight, gameObject.transform);
                vertex.transform.localPosition = vertices[i];
                vertex.transform.localScale = transform.lossyScale / (Mathf.Pow(transform.lossyScale.x, 2) * 20);
                vertex.GetComponent<VertexController>().AssignedVertices = vertexIndexes.ToArray();
                vertexObjects.Add(vertex);
                vertex.SetActive(false);
            }
        }

    }

    private void Update()
    {
        if (isSelected)
        {
            bool isPrimaryPressed = closeAction.action.IsPressed();
            bool isSecondaryPressed = openAction.action.IsPressed();

            if (isPrimaryPressed)
            {
                foreach (var item in vertexObjects)
                {
                    item.SetActive(false);
                }
            }
            else if (isSecondaryPressed)
            {
                foreach (var item in vertexObjects)
                {
                    item.SetActive(true);
                }
            }
        }
    }

    public void OnSelect()
    {
        if (isSelected)
        {
            isSelected = false;
            ObjectController.Instance.SelectedGameobject.Remove(gameObject);
            GetComponent<MeshRenderer>().material.color = Color.white;
        }
        else
        {
            isSelected = true;
            ObjectController.Instance.SelectedGameobject.Add(gameObject);
            GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
    }

    public void OnDiselect()
    {
        isSelected = false;
        ObjectController.Instance.SelectedGameobject.Remove(gameObject);
    }

}
