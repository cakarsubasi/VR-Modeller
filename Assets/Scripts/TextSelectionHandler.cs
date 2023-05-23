using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextSelectionHandler : MonoBehaviour
{
    public void HighlightText()
    {
        TextMeshProUGUI tm = gameObject.GetComponent<TextMeshProUGUI>();
        if(tm.color == new Color32(255, 255, 255, 255))
        {
            tm.color = new Color32(0, 255, 245, 255);
        }
        else
        {
            DehighlightText();
        }
        
    }

    public void DehighlightText()
    {
        TextMeshProUGUI tm = gameObject.GetComponent<TextMeshProUGUI>();
        tm.color = new Color32(255, 255, 255, 255);
    }

    public void SelectObject()
    {
        GameObject obj = gameObject.GetComponent<AssociatedObject>().GetAssociatedObject();
        /*if(obj.GetComponent<MeshController>().IsSelected)
        {
            obj.GetComponent<MeshController>().IsSelected = false;
        }
        else
        {
            obj.GetComponent<MeshController>().IsSelected = true;
        }*/
        GameObject mesh = obj.transform.FindChildWithTag("MeshObject").gameObject;
        ObjectController.Instance.OnSelect();
        ObjectController.Instance.OnMove();
        ObjectController.Instance.OnRotate();
        ObjectController.Instance.OnScale();
        mesh.GetComponent<MeshController>().OnSelect();
        ObjectController.Instance.OnSelect();
        ObjectController.Instance.OnMove();
        ObjectController.Instance.OnRotate();
        ObjectController.Instance.OnScale();

    }
}
