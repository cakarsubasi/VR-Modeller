using UnityEngine;
using UnityEngine.InputSystem;

public class LeftHandUIController : MonoBehaviour
{
    public GameObject UIParent;
    public InputActionProperty pincAction;

    private void Update()
    {
        bool pressed = pincAction.action.IsPressed();

        if (pressed)
        {
            UIParent.SetActive(true);
        }
        else
        {
            UIParent.SetActive(false);
        }
    }
}
