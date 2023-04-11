using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LeftHandUIController : MonoBehaviour
{
    public GameObject UICanvas;
    public InputActionProperty primaryAction;

    private void Update()
    {
        bool pressed = primaryAction.action.IsPressed();

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

  
