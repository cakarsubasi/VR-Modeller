using Meshes;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public class ObjectCreator : MonoBehaviour
{
    public GameObject GizmoPosition, GizmoRotation, GizmoScale;

    public void OnClickCreateGameObject(GameObject gameobject)
    {
        string name = gameobject.name + " " + (ObjectController.Instance.meshNum + 1);
        GameObject parent = new GameObject(name);
        ObjectController.Instance.meshNum++;

        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(Vector3.zero);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter += Camera.main.transform.forward * 4.0f;

        parent.transform.position = new Vector3(worldCenter.x, Camera.main.transform.position.y, worldCenter.z);
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

            case "Quad":
                meshController.SetupMeshController(CreateQuad);
                break;

            case "Cylinder":
                meshController.SetupMeshController(CreateCylinder);
                break;

            case "Sphere":
                meshController.SetupMeshController(CreateSphere);
                break;

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

    private static UMesh CreateCube()
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

    private static UMesh CreateCylinder()
    {
        return CreateCylinderNoCenterVertex();
    }

    private static UMesh CreateCylinderNoCenterVertex(int points = 16, float radius = 0.5f, float height = 2f)
    {
        UMesh mesh = UMesh.Create();

        Vertex[] verticesTop = new Vertex[points];
        for (int i = 0; i < points; ++i)
        {
            float angle = ((float)i / (float)points) * Mathf.PI * 2f;
            float x = math.cos(angle) * radius;
            float y = math.sin(angle) * radius;
            float3 position = new float3(x, height / 2f, y);
            verticesTop[i] = mesh.CreateVertex(position);
        }
        Face faceTop = mesh.CreateNGon(verticesTop);
        faceTop.FlipFace(true);

        Vertex[] verticesBottom = new Vertex[points];
        for (int i = 0; i < points; ++i)
        {
            float angle = ((float)i / (float)points) * Mathf.PI * 2f;
            float x = math.cos(angle) * radius;
            float y = math.sin(angle) * radius;
            float3 position = new float3(x, -height / 2f, y);
            verticesBottom[i] = mesh.CreateVertex(position);
        }
        Face faceBottom = mesh.CreateNGon(verticesBottom);

        for (int i = 0; i < points; ++i)
        {
            Vertex vert1 = verticesTop[i];
            Vertex vert2 = verticesTop[(i + 1) % points];
            Vertex vert3 = verticesBottom[(i + 1) % points];
            Vertex vert4 = verticesBottom[i];
            mesh.CreateQuad(new QuadElement<Vertex>(vert1, vert2, vert3, vert4));
        }

        return mesh;
    }

    private static UMesh CreateCylinderWithCenterVertex(int points = 16, float radius = 0.5f, float height = 2f)
    {
        UMesh mesh = UMesh.Create();

        Vertex vBottomCenter = mesh.CreateVertex((float3)new Vector3(0, -height / 2, 0));

        Vertex vTopCenter = mesh.CreateVertex((float3)new Vector3(0, height / 2, 0));

        Vertex[] vBottom = new Vertex[points];
        Vertex[] vTop = new Vertex[points];

        for (int i = 0; i < points; i++)
        {
            float angle = (Mathf.PI * 2 * i) / points;

            Vector3 vBottomPos = new Vector3(radius * Mathf.Cos(angle), -height / 2, radius * Mathf.Sin(angle));
            vBottom[i] = mesh.CreateVertex((float3)vBottomPos);

            Vector3 vTopPos = new Vector3(radius * Mathf.Cos(angle), height / 2, radius * Mathf.Sin(angle));
            vTop[i] = mesh.CreateVertex((float3)vTopPos);
        }

        for (int i = 0; i < points; i++)
        {
            int nextI = (i + 1) % points;
            mesh.CreateTriangle(new TriangleElement<Vertex>(vBottomCenter, vBottom[i], vBottom[nextI]));
            mesh.CreateTriangle(new TriangleElement<Vertex>(vTopCenter, vTop[nextI], vTop[i]));
            mesh.CreateQuad(new QuadElement<Vertex>(vTop[i], vTop[nextI], vBottom[nextI], vBottom[i]));
        }

        return mesh;
    }

    private static UMesh CreateQuad()
    {
        return CreateGrid(1, 1, 1f, 1f);
    }

    private static UMesh CreateGrid(int segmentsX = 1, int segmentsY = 1, float gridSizeX = 1f, float gridSizeY = 1f)
    {
        UMesh mesh = UMesh.Create();

        for (int i = 0; i <= segmentsX; i++)
        {
            for (int j = 0; j <= segmentsY; j++)
            {
                float x = (float)i / segmentsX * gridSizeX - (gridSizeX / 2);
                float y = 0;
                float z = (float)j / segmentsY * gridSizeY - (gridSizeY / 2);
                Vector3 position = new Vector3(x, y, z);
                mesh.CreateVertex(position);
            }
        }

        for (int i = 0; i < segmentsX; i++)
        {
            for (int j = 0; j < segmentsY; j++)
            {
                Vertex topLeft = mesh.Vertices[i * (segmentsY + 1) + j];
                Vertex topRight = mesh.Vertices[i * (segmentsY + 1) + j + 1];
                Vertex bottomLeft = mesh.Vertices[(i + 1) * (segmentsY + 1) + j];
                Vertex bottomRight = mesh.Vertices[(i + 1) * (segmentsY + 1) + j + 1];
                mesh.CreateQuad(new QuadElement<Vertex>(topLeft, topRight, bottomRight, bottomLeft));
            }
        }

        return mesh;
    }

    public static UMesh CreateSphere()
    {
        return CreateUVSphere(32, 16, 1f);
    }

    public static UMesh CreateUVSphere(int segments = 32, int rings = 16, float radius = 1f)
    {
        UMesh mesh = UMesh.Create();

        for (int i = 0; i <= segments; i++)
        {
            for (int j = 0; j <= rings; j++)
            {
                float circleRadius = sin(PI * j / rings) * radius;
                float2 circle;
                circle.x = sin(2f * PI * i / segments);
                circle.y = cos(2f * PI * i / segments);
                float x = circle.x * circleRadius;
                float y = cos(PI * j / rings) * radius;
                float z = circle.y * circleRadius;
                mesh.CreateVertex(new Vector3(x, y, z));
            }
        }

        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < rings; j++)
            {
                Vertex topLeft = mesh.Vertices[i * (rings + 1) + j];
                Vertex topRight = mesh.Vertices[i * (rings + 1) + j + 1];
                Vertex bottomLeft = mesh.Vertices[(i + 1) * (rings + 1) + j];
                Vertex bottomRight = mesh.Vertices[(i + 1) * (rings + 1) + j + 1];
                mesh.CreateQuad(new QuadElement<Vertex>(topLeft, topRight, bottomRight, bottomLeft));
            }
        }

        mesh.MergeByDistance();
        return mesh;
    }

}
