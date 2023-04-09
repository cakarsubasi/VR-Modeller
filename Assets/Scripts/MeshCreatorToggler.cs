using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCreatorToggler : MonoBehaviour
{
   public void toggleScroller()
    {
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }
}
