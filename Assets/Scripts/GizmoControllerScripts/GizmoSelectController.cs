using UnityEngine;

public class GizmoSelectController : MonoBehaviour
{
    private void Update()
    {
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        float minValue = 0.1f;
        float maxValue = 1000f;
        float scaleFactor = 0.025f;
        transform.localScale = Vector3.one * Mathf.Clamp(distance * scaleFactor, minValue, maxValue);
    }
}
