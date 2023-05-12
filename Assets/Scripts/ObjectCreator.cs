using Meshes;
using System;
using Unity.Mathematics;
using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    public GameObject GizmoPosition, GizmoRotation, GizmoScale;

    public void OnClickCreateGameObject(GameObject gameobject)
    {
        GameObject parent = new GameObject(gameobject.name);

        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(Vector3.zero);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter += Camera.main.transform.forward * 4.0f;

        parent.transform.position = worldCenter;
        parent.transform.localScale = gameobject.transform.localScale;

        GameObject go = Instantiate(gameobject, parent.transform);
        go.transform.localScale = Vector3.one;
        go.name = go.name.Replace("(Clone)", "");

        MeshController meshController = go.GetComponent<MeshController>();

        switch (go.name)
        {
            case "Cube":
                meshController.SetupMeshController(CreateCube);
                break;

            case "Capsule":
                throw new NotImplementedException { };

            case "Cylinder":
                throw new NotImplementedException { };

            case "Sphere":
                throw new NotImplementedException { };

            default:
                break;
        }

        GameObject pos = Instantiate(GizmoPosition, parent.transform);
        pos.name = pos.name.Replace("(Clone)", "");

        GameObject rot = Instantiate(GizmoRotation, parent.transform);
        rot.name = rot.name.Replace("(Clone)", "");

        GameObject sca = Instantiate(GizmoScale, parent.transform);
        sca.name = sca.name.Replace("(Clone)", "");

        ObjectController.Instance.OnSelect();
        ObjectController.Instance.OnMove();
        ObjectController.Instance.OnRotate();
        ObjectController.Instance.OnScale();

        go.GetComponent<MeshController>().OnSelect();

        ObjectController.Instance.OnSelect();
        ObjectController.Instance.OnMove();
        ObjectController.Instance.OnRotate();
        ObjectController.Instance.OnScale();
    }

    public void OnClickDeepCopyGameObject()
    {
        GameObject selectedGameObject = ObjectController.Instance.SelectedGameobject;

        GameObject parent = new GameObject(selectedGameObject.name + "Copy");

        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(Vector3.zero);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter += Camera.main.transform.forward * 4.0f;

        parent.transform.position = worldCenter;
        parent.transform.localScale = selectedGameObject.transform.parent.localScale;

        GameObject go = Instantiate(selectedGameObject, parent.transform);
        go.name = go.name.Replace("(Clone)", "");
        go.transform.localScale = selectedGameObject.transform.localScale;

        Destroy(go.transform.Find("VerticesParent").gameObject);


        MeshController meshController = go.GetComponent<MeshController>();

        meshController.SetupMeshController(selectedGameObject.GetComponent<MeshController>().EditableMesh.DeepCopy);

        GameObject pos = Instantiate(GizmoPosition, parent.transform);
        pos.name = pos.name.Replace("(Clone)", "");

        GameObject rot = Instantiate(GizmoRotation, parent.transform);
        rot.name = rot.name.Replace("(Clone)", "");

        GameObject sca = Instantiate(GizmoScale, parent.transform);
        sca.name = sca.name.Replace("(Clone)", "");

        ObjectController.Instance.OnSelect();
        ObjectController.Instance.OnMove();
        ObjectController.Instance.OnRotate();
        ObjectController.Instance.OnScale();

        go.GetComponent<MeshController>().OnSelect();

        ObjectController.Instance.OnSelect();
        ObjectController.Instance.OnMove();
        ObjectController.Instance.OnRotate();
        ObjectController.Instance.OnScale();
    }

    private UMesh CreateCube()
    {
        UMesh mesh = UMesh.Create();

        Vertex v1 = mesh.CreateVertex((float3)new Vector3(-1f, -1f, -1f));
        Vertex v2 = mesh.CreateVertex((float3)new Vector3(1f, -1f, -1f));
        Vertex v3 = mesh.CreateVertex((float3)new Vector3(1f, -1f, 1f));
        Vertex v4 = mesh.CreateVertex((float3)new Vector3(-1f, -1f, 1f));

        Vertex v5 = mesh.CreateVertex((float3)new Vector3(-1f, 1f, -1f));
        Vertex v6 = mesh.CreateVertex((float3)new Vector3(1f, 1f, -1f));
        Vertex v7 = mesh.CreateVertex((float3)new Vector3(1f, 1f, 1f));
        Vertex v8 = mesh.CreateVertex((float3)new Vector3(-1f, 1f, 1f));

        mesh.CreateQuad(new QuadElement<Vertex>(v1, v2, v3, v4));
        mesh.CreateQuad(new QuadElement<Vertex>(v8, v7, v6, v5));

        mesh.CreateQuad(new QuadElement<Vertex>(v1, v4, v8, v5));
        mesh.CreateQuad(new QuadElement<Vertex>(v1, v5, v6, v2));

        mesh.CreateQuad(new QuadElement<Vertex>(v2, v6, v7, v3));
        mesh.CreateQuad(new QuadElement<Vertex>(v3, v7, v8, v4));

        return mesh;
    }
}
