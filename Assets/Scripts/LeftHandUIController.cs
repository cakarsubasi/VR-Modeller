using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandUIController : MonoBehaviour
{
    public GameObject UICanvas;
    public InputActionProperty pincAction;

    private void Update()
    {
        bool pressed = pincAction.action.IsPressed();

        if (pressed)
        {
            UICanvas.SetActive(true);
        }
        else
        {
            UICanvas.SetActive(false);
        }
    }
}
