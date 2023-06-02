using Meshes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectController : MonoBehaviour
{
    public static ObjectController Instance;

    public Button deepCopyButton, deleteVerticesButton, mergeVerticesButton, flipAllButton, flipFacesButton, createFaceButton, changeColorButton, saveColorButton;

    public int meshNum = 0;

    List<GameObject> allObjects = new();
    GameObject selectedGameobject;
    bool selecting, moving, rotating, scaling;

    public GameObject SelectedGameobject
    {
        get => selectedGameobject;
        set
        {
            selectedGameobject = value;
            UpdateButtonInteractability();
        }
    }
    public List<GameObject> AllObjects { get => allObjects; set => allObjects = value; }
    public bool Selecting { get => selecting; set => selecting = value; }
    public bool Moving { get => moving; set => moving = value; }
    public bool Rotating { get => rotating; set => rotating = value; }
    public bool Scaling { get => scaling; set => scaling = value; }


    private void Awake()
    {
        Instance = this;
        UpdateButtonInteractability();
    }

    public void OnSelect()
    {
        if (selecting)
        {
            foreach (var item in AllObjects)
            {
                item.GetComponent<XRSimpleInteractable>().enabled = true;
                item.GetComponent<MeshCollider>().enabled = true;
            }
        }
        else
        {
            foreach (var item in AllObjects)
            {
                item.GetComponent<XRSimpleInteractable>().enabled = false;
                item.GetComponent<MeshCollider>().enabled = false;
            }
        }
    }

    public void OnMove()
    {
        if (selectedGameobject == null) return;
        if (moving)
        {
            selectedGameobject.transform.parent.FindChildWithTag("GizmoPosition").gameObject.SetActive(true);
        }
        else
        {
            selectedGameobject.transform.parent.FindChildWithTag("GizmoPosition").gameObject.SetActive(false);
        }
    }

    public void OnRotate()
    {
        if (selectedGameobject == null) return;
        if (rotating)
        {
            selectedGameobject.transform.parent.FindChildWithTag("GizmoRotation").gameObject.SetActive(true);
        }
        else
        {
            selectedGameobject.transform.parent.FindChildWithTag("GizmoRotation").gameObject.SetActive(false);
        }
    }

    public void OnScale()
    {
        if (selectedGameobject == null) return;
        if (scaling)
        {
            selectedGameobject.transform.parent.FindChildWithTag("GizmoScale").gameObject.SetActive(true);
        }
        else
        {
            selectedGameobject.transform.parent.FindChildWithTag("GizmoScale").gameObject.SetActive(false);
        }
    }

    public void ClearSelectedObject()
    {
        if (selectedGameobject == null) return;

        selectedGameobject.transform.parent.FindChildWithTag("GizmoPosition").gameObject.SetActive(false);
        selectedGameobject.transform.parent.FindChildWithTag("GizmoRotation").gameObject.SetActive(false);
        selectedGameobject.transform.parent.FindChildWithTag("GizmoScale").gameObject.SetActive(false);

        selectedGameobject.GetComponent<MeshController>().IsSelected = false;
        selectedGameobject.GetComponent<MeshController>().VerticesParent.SetActive(false);
        selectedGameobject = null;
    }


    public void UpdateButtonInteractability()
    {
        bool isDeepCopyInteractable = selectedGameobject != null;
        deepCopyButton.interactable = flipAllButton.interactable = changeColorButton.interactable = saveColorButton.interactable = isDeepCopyInteractable;

        bool isDeleteInteractable = selectedGameobject != null && selectedGameobject.GetComponent<MeshController>().ActiveVertices.Count != 0;
        deleteVerticesButton.interactable = isDeleteInteractable;

        bool isMergeInteractable = selectedGameobject != null && selectedGameobject.GetComponent<MeshController>().ActiveVertices.Count > 1;
        mergeVerticesButton.interactable = isMergeInteractable;

        if (selectedGameobject == null) return;

        List<Vertex> vertices = new List<Vertex>();
        HashSet<Face> faces = new HashSet<Face>();

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }
        selectedGameobject.GetComponent<MeshController>().EditableMesh.SelectFacesFromVertices(vertices, faces);

        bool isFaceSelected = faces.Count > 0;
        flipFacesButton.interactable = isFaceSelected;

        createFaceButton.interactable = !isFaceSelected && vertices.Count > 2;
    }

    public void OnClickDeleteVertices()
    {
        List<Vertex> vertices = new List<Vertex>();
        int vertxCount = selectedGameobject.GetComponent<MeshController>().EditableMesh.VertexCount;

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        selectedGameobject.GetComponent<MeshController>().EditableMesh.DeleteGeometry(vertices);
        selectedGameobject.GetComponent<MeshController>().EditableMesh.WriteAllToMesh();
        selectedGameobject.GetComponent<MeshCollider>().sharedMesh = selectedGameobject.GetComponent<MeshController>().EditableMesh.Mesh;
        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            Destroy(vertex);
        }
        selectedGameobject.GetComponent<MeshController>().ActiveVertices.Clear();

        if (vertices.Count == vertxCount)
        {
            Destroy(selectedGameobject.transform.parent.gameObject);
            selectedGameobject = null;
        }

        UpdateButtonInteractability();
    }

    public void OnClickMergeVertices()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        selectedGameobject.GetComponent<MeshController>().EditableMesh.MergeVertices(vertices);
        selectedGameobject.GetComponent<MeshController>().EditableMesh.WriteAllToMesh();
        selectedGameobject.GetComponent<MeshCollider>().sharedMesh = selectedGameobject.GetComponent<MeshController>().EditableMesh.Mesh;

        for (int i = selectedGameobject.GetComponent<MeshController>().ActiveVertices.Count - 1; i >= 1; i--)
        {
            Destroy(selectedGameobject.GetComponent<MeshController>().ActiveVertices[i]);
            selectedGameobject.GetComponent<MeshController>().ActiveVertices.RemoveAt(i);
        }
    }

    public void OnClickFlipNormals()
    {
        selectedGameobject.GetComponent<MeshController>().EditableMesh.FlipNormals();
        selectedGameobject.GetComponent<MeshController>().EditableMesh.WriteAllToMesh();
    }

    public void OnClickFlipFaces()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        HashSet<Face> faces = new HashSet<Face>();

        selectedGameobject.GetComponent<MeshController>().EditableMesh.SelectFacesFromVertices(vertices, faces);

        foreach (var face in faces)
        {
            face.FlipFace(true);
        }
        selectedGameobject.GetComponent<MeshController>().EditableMesh.WriteAllToMesh();
    }

    public void OnClickFlatShading(bool val)
    {
        if (val)
        {
            foreach (var item in AllObjects)
            {
                item.GetComponent<MeshController>().EditableMesh.Shading = ShadingType.Flat;
                item.GetComponent<MeshController>().EditableMesh.RecalculateAllAndWriteToMesh();
            }
        }

    }

    public void OnClickSmoothShading(bool val)
    {
        if (val)
        {
            foreach (var item in AllObjects)
            {
                item.GetComponent<MeshController>().EditableMesh.Shading = ShadingType.Smooth;
                item.GetComponent<MeshController>().EditableMesh.RecalculateAllAndWriteToMesh();
            }
        }
    }

    public void OnClickCreateFace()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        selectedGameobject.GetComponent<MeshController>().EditableMesh.CreateFace(vertices);
        selectedGameobject.GetComponent<MeshController>().EditableMesh.WriteAllToMesh();
        selectedGameobject.GetComponent<MeshCollider>().sharedMesh = selectedGameobject.GetComponent<MeshController>().EditableMesh.Mesh;
    }
}
