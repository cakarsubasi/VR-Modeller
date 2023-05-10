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
    public GameObject vertexHighlight;
    public InputActionProperty openOrCloseAction;

    List<Vector3> vertices = new List<Vector3>();
    List<GameObject> vertexObjects = new List<GameObject>();
    bool isSelected;
    public UMesh editableMesh;
    GameObject verticesParent;
    List<GameObject> activeVertices = new List<GameObject>();

    public delegate UMesh CreateMeshDelegate();

    public List<Vector3> Vertices { get => vertices; set => vertices = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public GameObject VerticesParent { get => verticesParent; set => verticesParent = value; }
    public List<GameObject> ActiveVertices { get => activeVertices; set => activeVertices = value; }

    public void SetupMeshController(CreateMeshDelegate createMeshFunction)
    {
        openOrCloseAction.action.performed += OpenVertices;
        ObjectController.Instance.AllObjects.Add(gameObject);


        editableMesh = createMeshFunction();
        GetComponent<MeshFilter>().mesh = editableMesh.Mesh;
        editableMesh.WriteAllToMesh();

        Vertices = editableMesh.VertexLocations.ToList();

        verticesParent = new GameObject("VerticesParent");
        verticesParent.transform.parent = this.transform;
        verticesParent.transform.localPosition = Vector3.zero;
        verticesParent.transform.localScale = Vector3.one;

        StartCoroutine(InitVertexObjects(Vertices, 0, false));
        StartCoroutine(Extrude());
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
            vertex.GetComponent<VertexController>().Vertex = editableMesh.Vertices[i];
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

    IEnumerator Extrude()
    {
        yield return new WaitUntil(() => activeVertices.Count != 0);
        yield return new WaitUntil(() => IsMovingVertex());

        GameObject movedVertex = MovedVertex();
        Vector3 currentVertexPos = transform.InverseTransformPoint(movedVertex.transform.position);

        int activeVertexCount = activeVertices.Count;
        List<Vertex> vertices = new List<Vertex>();

        int startVertexIndex = editableMesh.VertexCount;

        foreach (var vertex in activeVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        try
        {
            editableMesh.Extrude(vertices);
        }
        catch (Exception err)
        {
            Debug.LogError(err);
        }

        Vertices = editableMesh.VertexLocations.ToList();
        StartCoroutine(InitVertexObjects(Vertices, startVertexIndex, true));

        while (IsMovingVertex())
        {
            foreach (var vertex in activeVertices)
            {
                if (vertex == movedVertex) continue;

                vertex.transform.localPosition += (transform.InverseTransformPoint(movedVertex.transform.position) - currentVertexPos);
                vertex.GetComponent<VertexController>().Vertex.Position = (float3)vertex.transform.localPosition;
                editableMesh.WriteAllToMesh();

                if (GetComponent<MeshCollider>() != null)
                {
                    GetComponent<MeshCollider>().sharedMesh = editableMesh.Mesh;
                }
            }
            currentVertexPos = transform.InverseTransformPoint(movedVertex.transform.position);
            yield return null;
        }

        foreach (var vertex in activeVertices)
        {
            vertex.GetComponent<MeshRenderer>().material = vertex.GetComponent<VertexController>().normalMaterial;
            vertex.GetComponent<VertexController>().IsActivated = false;
        }

        activeVertices.Clear();

        StartCoroutine(Extrude());
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
}
