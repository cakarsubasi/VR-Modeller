using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectToggler : MonoBehaviour
{
    public void ToggleActiveState()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
