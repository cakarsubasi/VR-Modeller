using UnityEngine;

public class MeshCreatorToggler : MonoBehaviour
{
    public void ToggleScroller()
    {
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }
}
