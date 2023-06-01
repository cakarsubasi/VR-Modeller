using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MeshList : MonoBehaviour
{
    private TextMeshProUGUI tm;
    public GameObject go;
    public GameObject menu;
    public void CreateTextMesh(GameObject obj)
    {   
        string objectName = obj.name;
        GameObject tmObject = Instantiate(go, menu.transform);
        tm = tmObject.GetComponent<TextMeshProUGUI>();

        int children_num = menu.transform.childCount;
        tmObject.name = objectName + (children_num - 1);

        tm.text = tmObject.name;
        tm.fontSize = 12;

        tmObject.layer = 5;

        tmObject.transform.SetParent(menu.transform);
        tmObject.transform.localPosition = new Vector3(80, 140 - 40 * (children_num - 1), -5);
        tmObject.transform.localScale = new Vector3(2, 2, 2);
        tmObject.transform.rotation = new Quaternion(0, 0, 0, 0);

        string asso_obj_name = objectName + " " + (children_num - 1);
        GameObject asso_obj = GameObject.Find(asso_obj_name);
        tmObject.GetComponent<AssociatedObject>().SetAssociatedObject(asso_obj);

        if (asso_obj.transform.FindChildWithTag("MeshObject").gameObject.GetComponent<MeshController>().IsSelected)
        {
            tm.color = Color.cyan;
        }
    }
}
