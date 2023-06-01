using MeshesIO;
using System.IO;
using TMPro;
using UnityEngine;

public class FileObject : MonoBehaviour
{
    public TextMeshProUGUI fileText;
    public GameObject GizmoPosition, GizmoRotation, GizmoScale, GizmoSelect, gameobject;

    SceneDescription scene;

    public void OnClickCreate()
    {
        string str = File.ReadAllText(fileText.text);
        scene = WavefrontIO.Parse(str);

        int count = 1;
        float offsetX = 0f;

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
            pos.transform.localScale /= 2;

            GameObject rot = Instantiate(GizmoRotation, parent.transform);
            rot.name = rot.name.Replace("(Clone)", "");
            rot.transform.localScale /= 2;

            GameObject sca = Instantiate(GizmoScale, parent.transform);
            sca.name = sca.name.Replace("(Clone)", "");
            sca.transform.localScale /= 2;

            GameObject select = Instantiate(GizmoSelect, parent.transform);
            select.name = select.name.Replace("(Clone)", "");
            select.transform.localScale /= 2;

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
            offsetX += 2f;
        }
    }

}
