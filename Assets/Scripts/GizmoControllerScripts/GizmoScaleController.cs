using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GizmoScaleController : MonoBehaviour
{
    Transform targetObject;

    public GameObject xHandle;
    public GameObject yHandle;
    public GameObject zHandle;
    public GameObject centerHandle;

    public float scaleSpeed;

    GameObject selectedHandle;
    Vector3 previousPosition;
    Vector3 baseScale, baseScaleForCenter;

    private void Start()
    {
        targetObject = transform.parent.FindChildWithTag("MeshObject");
        baseScale = xHandle.transform.localScale;
        baseScaleForCenter = centerHandle.transform.localScale;

        xHandle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabX);
        xHandle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        yHandle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabY);
        yHandle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        zHandle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabZ);
        zHandle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        centerHandle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabCenter);
        centerHandle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);
    }

    private void Update()
    {
        if (selectedHandle != null)
        {
            Vector3 controllerPosition = selectedHandle.GetComponent<XRSimpleInteractable>().firstInteractorSelecting.transform.position;
            Vector3 controllerDelta = controllerPosition - previousPosition;


            if (selectedHandle == xHandle)
            {
                float scaleDelta = controllerDelta.x * scaleSpeed;
                targetObject.localScale = new Vector3(targetObject.localScale.x + scaleDelta, targetObject.localScale.y, targetObject.localScale.z);
                xHandle.transform.localScale = new Vector3(xHandle.transform.localScale.x, xHandle.transform.localScale.y, xHandle.transform.localScale.z + scaleDelta);
            }
            else if (selectedHandle == yHandle)
            {
                float scaleDelta = controllerDelta.y * scaleSpeed;
                targetObject.localScale = new Vector3(targetObject.localScale.x, targetObject.localScale.y + scaleDelta, targetObject.localScale.z);
                yHandle.transform.localScale = new Vector3(yHandle.transform.localScale.x, yHandle.transform.localScale.y, yHandle.transform.localScale.z + scaleDelta);
            }
            else if (selectedHandle == zHandle)
            {
                float scaleDelta = controllerDelta.z * scaleSpeed;
                targetObject.localScale = new Vector3(targetObject.localScale.x, targetObject.localScale.y, targetObject.localScale.z + scaleDelta);
                zHandle.transform.localScale = new Vector3(zHandle.transform.localScale.x, zHandle.transform.localScale.y, zHandle.transform.localScale.z + scaleDelta);
            }
            else if (selectedHandle == centerHandle)
            {
                float scaleDelta = (controllerDelta.x * scaleSpeed + controllerDelta.y * scaleSpeed + controllerDelta.z * scaleSpeed) / 3f;
                targetObject.localScale = new Vector3(targetObject.localScale.x + scaleDelta, targetObject.localScale.y + scaleDelta, targetObject.localScale.z + scaleDelta);
                centerHandle.transform.localScale += new Vector3(scaleDelta, scaleDelta, scaleDelta);
            }

            previousPosition = controllerPosition;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float minValue = 0.35f;
            float maxValue = 1000f;
            float scaleFactor = 0.25f;
            transform.localScale = Vector3.one * Mathf.Clamp(distance * scaleFactor, minValue, maxValue);
        }
    }


    private void OnGrabX(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = xHandle;
    }

    private void OnGrabY(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = yHandle;
    }

    private void OnGrabZ(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = zHandle;
    }

    private void OnGrabCenter(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = centerHandle;
    }

    private void OnRelease(SelectExitEventArgs eventArgs)
    {
        selectedHandle = null;
        xHandle.transform.localScale = baseScale;
        yHandle.transform.localScale = baseScale;
        zHandle.transform.localScale = baseScale;
        centerHandle.transform.localScale = baseScaleForCenter;
    }
}
