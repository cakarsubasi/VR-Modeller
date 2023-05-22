using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextSelectionHandler : MonoBehaviour
{
    public void HighlightText()
    {
        TextMeshPro tm = gameObject.GetComponent<TextMeshPro>();
        tm.color = new Color32(0, 255, 245, 255);
    }

    public void DehighlightText()
    {
        TextMeshPro tm = gameObject.GetComponent<TextMeshPro>();
        tm.color = new Color32(255, 255, 255, 255);
    }
}
