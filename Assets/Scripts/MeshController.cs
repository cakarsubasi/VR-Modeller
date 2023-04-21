using Meshes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshController : MonoBehaviour
{
    public GameObject vertexHighlight;
    public InputActionProperty openOrCloseAction;

    Mesh mesh;
    MeshFilter meshFilter;
    List<Vector3> vertices = new List<Vector3>();
    List<GameObject> vertexObjects = new List<GameObject>();
    bool isSelected;
    EditableMeshImpl editableMesh;
    GameObject verticesParent;

    public List<Vector3> Vertices { get => vertices; set => vertices = value; }
    public Mesh Mesh { get => mesh; set => mesh = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public GameObject VerticesParent { get => verticesParent; set => verticesParent = value; }

    private void Start()
    {
        openOrCloseAction.action.performed += OpenVertices;
        ObjectController.Instance.AllObjects.Add(gameObject);

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
        verticesParent = new GameObject("VerticesParent");
        verticesParent.transform.parent = this.transform;
        verticesParent.transform.localPosition = Vector3.zero;
        verticesParent.transform.localScale = Vector3.one;
        verticesParent.SetActive(false);
        int i = 0;
        while (i < vertices.Count)
        {
            GameObject vertex = Instantiate(vertexHighlight, verticesParent.transform);
            vertex.transform.localPosition = vertices[i];
            vertex.transform.localScale = transform.lossyScale / (Mathf.Pow(transform.lossyScale.x, 2) * 20);
            vertex.GetComponent<VertexController>().VertexIndex = i;
            vertexObjects.Add(vertex);

            i++;
            yield return null;
        }
    }


    public void OpenVertices(InputAction.CallbackContext context)
    {
        if (IsSelected)
        {
            bool isOpen = verticesParent.gameObject.activeInHierarchy;
            verticesParent.SetActive(!isOpen);
        }
    }

    public void OnSelect()
    {
        if (IsSelected)
        {
            IsSelected = false;
            ObjectController.Instance.SelectedGameobject = null;
            GetComponent<MeshRenderer>().material.color = Color.white;
        }
        else
        {
            ObjectController.Instance.ClearSelectedObject();
            IsSelected = true;
            ObjectController.Instance.SelectedGameobject = gameObject;
            GetComponent<MeshRenderer>().material.color = Color.cyan;
        }
    }
}
