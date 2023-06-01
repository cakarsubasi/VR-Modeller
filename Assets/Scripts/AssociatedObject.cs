using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssociatedObject : MonoBehaviour
{
    public GameObject associatedObject;
    public GameObject GetAssociatedObject()
    {
        return associatedObject;
    }
    public void SetAssociatedObject(GameObject obj)
    {
        associatedObject = obj;
    }
}
