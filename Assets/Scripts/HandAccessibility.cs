using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HandAccessibility : MonoBehaviour
{
    public GameObject menu;
    public Transform RightHandController;
    public Transform LeftHandController;
    public ToggleGroup toggleGroup;

    public Toggle GetSelectedToggle()
    {
        Toggle[] toggles = toggleGroup.GetComponentsInChildren<Toggle>();
        foreach (var t in toggles)
            if (t.isOn) return t;  
        return null;           
    }
    public void changeParent()
    {
        Toggle toggle = GetSelectedToggle();

        if(toggle.tag == "LeftHand")
        {
            menu.transform.SetParent(LeftHandController, false);
        }
        else
        {
            menu.transform.SetParent(RightHandController, false);
        }
    }

}
