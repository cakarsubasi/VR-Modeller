using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    public Image selectedColor;
    public TMP_InputField rValue, gValue, bValue;
    float r = 1f;
    float g = 1f;
    float b = 1f;

    public void OnRValueChanged(float value)
    {
        r = value / 255f;
        rValue.text = value.ToString();
        selectedColor.color = new Color(r, g, b);
    }

    public void OnGValueChanged(float value)
    {
        g = value / 255f;
        gValue.text = value.ToString();
        selectedColor.color = new Color(r, g, b);
    }

    public void OnBValueChanged(float value)
    {
        b = value / 255f;
        bValue.text = value.ToString();
        selectedColor.color = new Color(r, g, b);
    }

    public void OnClickSave()
    {
        ObjectController.Instance.SelectedGameobject.GetComponent<MeshController>().SetColor(new Color(r, g, b));
    }

}
