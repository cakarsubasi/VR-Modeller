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

        GameObject pos = Instantiate(GizmoPosition, parent.transform);
        pos.name = pos.name.Replace("(Clone)", "");

        GameObject rot = Instantiate(GizmoRotation, parent.transform);
        rot.name = rot.name.Replace("(Clone)", "");

        GameObject sca = Instantiate(GizmoScale, parent.transform);
        sca.name = sca.name.Replace("(Clone)", "");
    }
}
