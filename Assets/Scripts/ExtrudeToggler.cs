using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ExtrudeToggler : MonoBehaviour
{
    public Toggle extrudeToggle;
    public InputActionProperty activateExtrudeInput;

    private void Start()
    {
        activateExtrudeInput.action.performed += ActivateExtrude;
        extrudeToggle.isOn = GameManager.Instance.Extrude;
    }

    public void SetExtrude(bool value)
    {
        GameManager.Instance.Extrude = value;
    }

    public void ActivateExtrude(InputAction.CallbackContext context)
    {
        bool isActive = GameManager.Instance.Extrude;
        GameManager.Instance.Extrude = extrudeToggle.isOn = !isActive;
    }
}
