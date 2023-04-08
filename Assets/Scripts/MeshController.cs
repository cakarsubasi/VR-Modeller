using Meshes;
using System.Collections;
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
    EditableMeshImpl editableMesh;

    public List<Vector3> Vertices { get => vertices; set => vertices = value; }
    public Mesh Mesh { get => mesh; set => mesh = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }

    private void Start()
    {
        editableMesh = GetComponent<EditableMesh>().MeshInternal;
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        //Vertices = mesh.vertices.ToList();
        Vertices = editableMesh.Vertices.ToList();
        //Debug.Log("Vertices: " + Vertices.Count);

        List<Vector3> updatedVertices = new List<Vector3>();

        StartCoroutine(InitVertexObjects(Vertices));

        /*for (var i = 0; i < Vertices.Count; i++)
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
        }*/

    }

    IEnumerator InitVertexObjects(List<Vector3> vertices)
    {
        List<Vector3> updatedVertices = new List<Vector3>();

        //Debug.Log("Strarted: " + vertices.Count);

        int i = 0;
        while (i < vertices.Count)
        {
            List<Vector3> sameVertices = vertices.FindAll(x => x == vertices[i]);
            //Debug.Log("i: " + i);
            if (!updatedVertices.Contains(sameVertices[0]))
            {
                updatedVertices.Add(sameVertices[0]);

                List<int> vertexIndexes = new List<int>();

                int k = 0;
                while (k < vertices.Count)
                {
                    //Debug.Log("k: " + k);
                    if (vertices[k] == sameVertices[0])
                    {
                        vertexIndexes.Add(k);
                        //Debug.Log(sameVertices[0] + " " + k);
                    }
                    k++;
                    yield return null;
                }
                GameObject vertex = Instantiate(vertexHighlight, gameObject.transform);
                vertex.transform.localPosition = vertices[i];
                vertex.transform.localScale = transform.lossyScale / (Mathf.Pow(transform.lossyScale.x, 2) * 20);
                vertex.GetComponent<VertexController>().AssignedVertices = vertexIndexes.ToArray();
                vertexObjects.Add(vertex);
                vertex.SetActive(false);
                Debug.Log("Instantiated vertex");
            }
            i++;
            yield return null;
        }
    }

    private void Update()
    {
        if (IsSelected)
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
        if (IsSelected)
        {
            IsSelected = false;
            ObjectController.Instance.SelectedGameobject.Remove(gameObject);
            GetComponent<MeshRenderer>().material.color = Color.white;
        }
        else
        {
            IsSelected = true;
            ObjectController.Instance.SelectedGameobject.Add(gameObject);
            GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
    }

    public void OnDiselect()
    {
        IsSelected = false;
        ObjectController.Instance.SelectedGameobject.Remove(gameObject);
    }

}
