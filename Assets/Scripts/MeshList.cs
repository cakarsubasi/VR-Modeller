using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeshList : MonoBehaviour
{
    private TextMeshPro tm;
    private GameObject tmObject;
    public GameObject menu;
    public void CreateTextMesh(GameObject gameObject)
    {
        
        string objectName = gameObject.name;
        tmObject = new GameObject(objectName);
        tm = tmObject.AddComponent<TextMeshPro>();
        tmObject.AddComponent<CanvasRenderer>();

        int children_num = menu.transform.childCount;
        tmObject.name = objectName + children_num;

        tm.text = tmObject.name;
        tm.fontSize = 12;

        tmObject.layer = 5;

        tmObject.transform.SetParent(menu.transform);
        tmObject.transform.localPosition = new Vector3(35, 100 - 20 * (children_num - 1), -5);
        tmObject.transform.localScale = new Vector3(15, 15, 15);
        tmObject.transform.rotation = new Quaternion(0, 0, 0, 0);

        //GameObject go = GameObject.Instantiate(tmObject);
    }
}
