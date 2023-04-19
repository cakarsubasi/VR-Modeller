using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GizmoRotationController : MonoBehaviour
{
    Transform targetObject;

    public GameObject xCircle;
    public GameObject yCircle;
    public GameObject zCircle;

    public bool snapping = true; //Get it from GameSettings
    public int snapAngle;
    public float rotationSpeed = 1.0f;

    GameObject selectedCircle;
    Vector3 previousPosition;
    Transform gizmoPosition;

    private void Start()
    {
        targetObject = transform.parent;
        gizmoPosition = transform.parent.FindChildWithTag("GizmoPosition");

        xCircle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabX);
        xCircle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        yCircle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabY);
        yCircle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        zCircle.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabZ);
        zCircle.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);
    }

    private void Update()
    {
        if (selectedCircle != null)
        {
            Vector3 controllerPosition = selectedCircle.GetComponent<XRSimpleInteractable>().firstInteractorSelecting.transform.position;
            Vector3 controllerDelta = controllerPosition - previousPosition;

            Vector3 cameraForward = Camera.main.transform.forward;

            float angle;
            if (Mathf.Abs(cameraForward.z) > Mathf.Abs(cameraForward.x))
            {
                angle = -controllerDelta.x * rotationSpeed * 180 / Mathf.PI;
            }
            else
            {
                angle = controllerDelta.z * rotationSpeed * 180 / Mathf.PI;
            }

            if (selectedCircle == xCircle)
            {
                targetObject.Rotate(angle, 0, 0, Space.World);
            }
            else if (selectedCircle == yCircle)
            {
                targetObject.Rotate(0, angle, 0, Space.World);
            }
            else if (selectedCircle == zCircle)
            {
                targetObject.Rotate(0, 0, angle, Space.World);
            }

            previousPosition = controllerPosition;
        }
    }

    private void OnGrabX(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedCircle = xCircle;
        xCircle.transform.SetParent(targetObject.transform);
        transform.SetParent(null);
        gizmoPosition.SetParent(null);
    }

    private void OnGrabY(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedCircle = yCircle;
        yCircle.transform.SetParent(targetObject.transform);
        transform.SetParent(null);
        gizmoPosition.SetParent(null);
    }

    private void OnGrabZ(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedCircle = zCircle;
        zCircle.transform.SetParent(targetObject.transform);
        transform.SetParent(null);
        gizmoPosition.SetParent(null);
    }

    private void OnRelease(SelectExitEventArgs eventArgs)
    {
        selectedCircle = null;
        xCircle.transform.SetParent(transform);
        yCircle.transform.SetParent(transform);
        zCircle.transform.SetParent(transform);

        if (snapping)
        {
            Vector3 eulerAngles = targetObject.rotation.eulerAngles;
            eulerAngles.x = Mathf.Round(eulerAngles.x / snapAngle) * snapAngle;
            eulerAngles.y = Mathf.Round(eulerAngles.y / snapAngle) * snapAngle;
            eulerAngles.z = Mathf.Round(eulerAngles.z / snapAngle) * snapAngle;
            targetObject.rotation = Quaternion.Euler(eulerAngles);
            xCircle.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            yCircle.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
            zCircle.transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }

        transform.SetParent(targetObject.transform);
        gizmoPosition.SetParent(targetObject.transform);
    }
}
