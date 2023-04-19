using UnityEngine;

public class ObjectCreator : MonoBehaviour
{
    public GameObject GizmoPosition, GizmoRotation;

    public void OnClickCreateGameObject(GameObject gameobject)
    {
        GameObject go = Instantiate(gameobject);

        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(Vector3.zero);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter += Camera.main.transform.forward * 4.0f;

        go.transform.position = worldCenter;

        GameObject pos = Instantiate(GizmoPosition, go.transform);
        pos.name = pos.name.Replace("(Clone)", "");

        GameObject rot = Instantiate(GizmoRotation, go.transform);
        rot.name = rot.name.Replace("(Clone)", "");
    }
}
