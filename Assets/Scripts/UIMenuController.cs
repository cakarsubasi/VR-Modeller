using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIMenuController : MonoBehaviour
{
    public InputActionReference primaryButton = null;

    private void Awake()
    {
        primaryButton.action.performed += UIMenu;
    }

    private void OnDestroy()
    {
        primaryButton.action.performed -= UIMenu;
    }

    private void UIMenu(InputAction.CallbackContext context)
    {
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }


}
