using UnityEngine;

public class MeshCreater : MonoBehaviour
{
    public void OnClickCreateGameObject(GameObject gameobject)
    {
        GameObject go = Instantiate(gameobject);

        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(Vector3.zero);
        Vector3 worldCenter = Camera.main.ScreenToWorldPoint(screenCenter);
        worldCenter += Camera.main.transform.forward * 4.0f;

        go.transform.position = worldCenter;
    }
}
