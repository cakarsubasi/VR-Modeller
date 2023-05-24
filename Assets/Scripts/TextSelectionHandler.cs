using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class TextSelectionHandler : MonoBehaviour
{
    GameObject ray;
    Vector3 initialPos;
    Color cur_color;
    public void Start()
    {
        ray = GameObject.Find("RightHand Ray");
        initialPos = transform.localPosition;
        cur_color = Color.white;
    }
    public void HighlightText()
    {
        TextMeshProUGUI tm = gameObject.GetComponent<TextMeshProUGUI>();
        if(tm.color == cur_color)
        {
            tm.color = Color.cyan;
        }
        else
        {
            DehighlightText();
        }
        
    }

    public void DehighlightText()
    {
        TextMeshProUGUI tm = gameObject.GetComponent<TextMeshProUGUI>();
        tm.color = cur_color;
    }

    public void SelectObject()
    {
        GameObject obj = gameObject.GetComponent<AssociatedObject>().GetAssociatedObject();
        
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

    public void BeginDrag()
    {
        gameObject.transform.localPosition += new Vector3(-100, 0, 0);
    }
    public void DragText()
    {
        Vector3 previousPosition = gameObject.transform.position;
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 controllerPosition = ray.GetComponent<LineRenderer>().GetPosition(1);
        Vector3 controllerDelta = controllerPosition - previousPosition;
        controllerDelta *= distance;

        gameObject.transform.position += new Vector3(controllerDelta.x, controllerDelta.y, 0);
    }

    public void DropText()
    {
        GameObject hmenu = GameObject.FindGameObjectWithTag("HMenu");
        foreach(Transform child in hmenu.transform)
        {
            if(child.name != "Image" && child != transform)
            {
                if (CheckBound(child))
                {
                    gameObject.GetComponent<AssociatedObject>().GetAssociatedObject().transform.SetParent(child.GetComponent<AssociatedObject>().GetAssociatedObject().transform);
                    TextMeshProUGUI tm = child.GetComponent<TextMeshProUGUI>();
                    if(tm.color == Color.cyan || tm.color == Color.white)
                    {
                        Color new_color = new Color(
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f),
                            Random.Range(0f, 1f)
                        );
                        tm.color = new_color;
                        gameObject.GetComponent<TextMeshProUGUI>().color = new_color;
                        cur_color = new_color;
                        child.GetComponent<TextSelectionHandler>().cur_color = new_color;
                    }
                    else
                    {
                        gameObject.GetComponent<TextMeshProUGUI>().color = child.GetComponent<TextSelectionHandler>().cur_color;
                    }
                }
            }
        }
        transform.localPosition = initialPos;

    }

    public bool CheckBound(Transform tr)
    {
        if((tr.localPosition.x + 20 > transform.localPosition.x && tr.localPosition.x - 20 < transform.localPosition.x) 
            && (tr.localPosition.y + 20 > transform.localPosition.y && tr.localPosition.y - 20 < transform.localPosition.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }


}
