using Meshes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeshController : MonoBehaviour
{
    public GameObject vertexHighlight, activeVerticesObject;
    public InputActionProperty openOrCloseAction;

    List<Vector3> vertices = new List<Vector3>();
    List<GameObject> vertexObjects = new List<GameObject>();
    bool isSelected;
    UMesh editableMesh;
    GameObject verticesParent;
    List<GameObject> activeVertices = new List<GameObject>();
    GameObject objectInActiveVertices;

    public delegate UMesh CreateMeshDelegate();

    public List<Vector3> Vertices { get => vertices; set => vertices = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public GameObject VerticesParent { get => verticesParent; set => verticesParent = value; }
    public List<GameObject> ActiveVertices { get => activeVertices; set => activeVertices = value; }
    public UMesh EditableMesh { get => editableMesh; set => editableMesh = value; }

    public void SetupMeshController(CreateMeshDelegate createMeshFunction)
    {
        openOrCloseAction.action.performed += ClearActivedVerticesOpenOrCloseVertices;
        ObjectController.Instance.AllObjects.Add(gameObject);


        EditableMesh = createMeshFunction();
        GetComponent<MeshFilter>().mesh = EditableMesh.Mesh;
        EditableMesh.WriteAllToMesh();

        Vertices = EditableMesh.VertexLocations.ToList();
        verticesParent = new GameObject("VerticesParent");
        verticesParent.transform.parent = this.transform;
        verticesParent.transform.localPosition = Vector3.zero;
        verticesParent.transform.localScale = Vector3.one;
        verticesParent.transform.localRotation = Quaternion.identity;

        StartCoroutine(InitVertexObjects(Vertices, 0, false));
        StartCoroutine(MoveMultpleVertices());
    }

    IEnumerator InitVertexObjects(List<Vector3> vertices, int startIndex, bool showVertices)
    {
        verticesParent.SetActive(showVertices);
        int i = startIndex;
        while (i < vertices.Count)
        {
            GameObject vertex = Instantiate(vertexHighlight, verticesParent.transform);
            vertex.transform.localPosition = vertices[i];
            vertex.transform.localScale = transform.lossyScale / (Mathf.Pow(transform.lossyScale.x, 2) * 20);
            vertex.GetComponent<VertexController>().Vertex = EditableMesh.Vertices[i];
            vertexObjects.Add(vertex);

            i++;
            yield return null;
        }
    }


    public void ClearActivedVerticesOpenOrCloseVertices(InputAction.CallbackContext context)
    {
        bool isOpen = verticesParent.gameObject.activeInHierarchy;

        if (activeVertices.Count != 0)
        {
            foreach (var vertex in activeVertices)
            {
                vertex.GetComponent<MeshRenderer>().material = vertex.GetComponent<VertexController>().normalMaterial;
                vertex.GetComponent<VertexController>().IsActivated = false;
            }

            activeVertices.Clear();

            if (isOpen) return;
        }

        if (IsSelected)
        {
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

    IEnumerator MoveMultpleVertices()
    {
        yield return new WaitUntil(() => activeVertices.Count != 0);
        yield return new WaitUntil(() => IsMovingVertex());

        GameObject movedVertex = MovedVertex();
        Vector3 currentVertexPos = transform.InverseTransformPoint(movedVertex.transform.position);

        if (GameManager.Instance.Extrude)
        {

            int activeVertexCount = activeVertices.Count;
            List<Vertex> vertices = new List<Vertex>();

            int startVertexIndex = EditableMesh.VertexCount;

            foreach (var vertex in activeVertices)
            {
                vertices.Add(vertex.GetComponent<VertexController>().Vertex);
            }

            try
            {
                EditableMesh.Extrude(vertices);
            }
            catch (Exception err)
            {
                Debug.LogError(err);
            }

            Vertices = EditableMesh.VertexLocations.ToList();
            StartCoroutine(InitVertexObjects(Vertices, startVertexIndex, true));
        }

        while (IsMovingVertex())
        {
            foreach (var vertex in activeVertices)
            {
                if (vertex == movedVertex) continue;

                vertex.transform.localPosition += (transform.InverseTransformPoint(movedVertex.transform.position) - currentVertexPos);
                vertex.GetComponent<VertexController>().Vertex.Position = (float3)vertex.transform.localPosition;
                EditableMesh.WriteAllToMesh();

                if (GetComponent<MeshCollider>() != null)
                {
                    GetComponent<MeshCollider>().sharedMesh = EditableMesh.Mesh;
                }
            }
            currentVertexPos = transform.InverseTransformPoint(movedVertex.transform.position);
            yield return null;
        }


        StartCoroutine(MoveMultpleVertices());
    }

    private bool IsMovingVertex()
    {
        foreach (var vertex in activeVertices)
        {
            if (vertex.GetComponent<VertexController>().IsSelected)
            {
                return true;
            }
        }
        return false;
    }

    private GameObject MovedVertex()
    {
        foreach (var vertex in activeVertices)
        {
            if (vertex.GetComponent<VertexController>().IsSelected)
            {
                return vertex;
            }
        }
        return null;
    }

    public void CreateObjectInActiatedVertices()
    {

        if (objectInActiveVertices != null)
        {
            Destroy(objectInActiveVertices);
        }

        if (activeVertices.Count < 3) { return; }

        Vector3 avaragePosition = Vector3.zero;

        foreach (var vertex in activeVertices)
        {
            avaragePosition += vertex.transform.position;
        }
        avaragePosition /= activeVertices.Count;


        objectInActiveVertices = Instantiate(activeVerticesObject, gameObject.transform);
        objectInActiveVertices.transform.position = avaragePosition;

        HashSet<Face> faces = new();
        HashSet<Edge> edges = new();
        HashSet<Vertex> vertices = new();

        foreach (var vertex in activeVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        editableMesh.SelectFacesFromVertices(vertices, faces);
        editableMesh.SelectEdgesFromVertices(vertices, edges);


        UMesh mesh = editableMesh.CopySelectionToNewMesh(vertices, edges, faces);

        objectInActiveVertices.GetComponent<MeshFilter>().mesh = mesh.Mesh;
        objectInActiveVertices.GetComponent<MeshCollider>().sharedMesh = mesh.Mesh;
        mesh.WriteAllToMesh();

        objectInActiveVertices.transform.localScale *= 0.5f;

    }
}
