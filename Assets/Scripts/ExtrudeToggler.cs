using UnityEngine;
using UnityEngine.UI;

public class ExtrudeToggler : MonoBehaviour
{
    public Toggle extrudeToggle;

    private void Start()
    {
        extrudeToggle.isOn = GameManager.Instance.Extrude;
    }

    public void SetExtrude(bool value)
    {
        GameManager.Instance.Extrude = value;
    }
}
