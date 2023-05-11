using Meshes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectController : MonoBehaviour
{
    public static ObjectController Instance;

    public Button deepCopyButton, deleteButton;

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
        selectedGameobject.GetComponent<MeshRenderer>().material.color = Color.white;
        selectedGameobject.GetComponent<MeshController>().VerticesParent.SetActive(false);
        selectedGameobject = null;
    }


    public void UpdateButtonInteractability()
    {
        bool isDeepCopyInteractable = selectedGameobject != null;
        deepCopyButton.interactable = isDeepCopyInteractable;

        bool isDeleteInteractable = selectedGameobject != null && selectedGameobject.GetComponent<MeshController>().ActiveVertices.Count != 0;
        deleteButton.interactable = isDeleteInteractable;
    }

    public void OnClickDelete()
    {
        List<Vertex> vertices = new List<Vertex>();

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            vertices.Add(vertex.GetComponent<VertexController>().Vertex);
        }

        selectedGameobject.GetComponent<MeshController>().EditableMesh.DeleteVertices(vertices);
        selectedGameobject.GetComponent<MeshController>().EditableMesh.OptimizeRendering();
        selectedGameobject.GetComponent<MeshController>().EditableMesh.WriteAllToMesh();

        foreach (var vertex in selectedGameobject.GetComponent<MeshController>().ActiveVertices)
        {
            Destroy(vertex);
        }
        selectedGameobject.GetComponent<MeshController>().ActiveVertices.Clear();

        deleteButton.interactable = false;

        Debug.Log(vertices.Count + " " + selectedGameobject.GetComponent<MeshController>().EditableMesh.VertexCount);

        if (vertices.Count == selectedGameobject.GetComponent<MeshController>().EditableMesh.VertexCount)
        {
            //Destroy(selectedGameobject.transform.parent.gameObject);
        }
    }
}
