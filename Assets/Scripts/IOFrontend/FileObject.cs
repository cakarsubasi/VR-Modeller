using MeshesIO;
using System.IO;
using TMPro;
using UnityEngine;

public class FileObject : MonoBehaviour
{
    public TextMeshProUGUI fileText;
    public GameObject GizmoPosition, GizmoRotation, GizmoScale, gameobject;

    SceneDescription scene;

    public void OnClickCreate()
    {
        string str = File.ReadAllText(fileText.text);

        scene = WavefrontIO.Parse(str);

        int count = 1;
        float offsetX = 5f;

        foreach (var item in scene.objects)
        {
            string name = Path.GetFileNameWithoutExtension(fileText.text + count) + " " + (ObjectController.Instance.meshNum + 1);
            GameObject parent = new GameObject(name);
            ObjectController.Instance.meshNum++;

            Vector3 screenCenter = Camera.main.ViewportToScreenPoint(Vector3.zero);
            Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
            worldCenter += Camera.main.transform.forward * 4.0f;

            parent.transform.position = new Vector3(worldCenter.x + offsetX, Camera.main.transform.position.y, worldCenter.z);
            parent.transform.localScale = gameobject.transform.localScale;

            GameObject go = Instantiate(gameobject, parent.transform);
            go.transform.localScale = Vector3.one;
            go.name = go.name.Replace("(Clone)", "");

            MeshController meshController = go.GetComponent<MeshController>();
            meshController.SetupMeshController(item);

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

            count++;
            offsetX += offsetX;
        }
    }

}
