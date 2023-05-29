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
    public float rotationSpeed;

    GameObject selectedCircle;
    Vector3 previousPosition;

    private void Start()
    {
        targetObject = transform.parent.FindChildWithTag("MeshObject");

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
                xCircle.transform.Rotate(angle, 0, 0, Space.World);
            }
            else if (selectedCircle == yCircle)
            {
                targetObject.Rotate(0, angle, 0, Space.World);
                yCircle.transform.Rotate(0, angle, 0, Space.World);
            }
            else if (selectedCircle == zCircle)
            {
                targetObject.Rotate(0, 0, angle, Space.World);
                zCircle.transform.Rotate(0, 0, angle, Space.World);
            }

            previousPosition = controllerPosition;
        }
        else
        {
            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float minValue = 0.25f;
            float maxValue = 1000f;
            float scaleFactor = 0.15f;
            transform.localScale = Vector3.one * Mathf.Clamp(distance * scaleFactor, minValue, maxValue);
        }
    }

    private void OnGrabX(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedCircle = xCircle;
    }

    private void OnGrabY(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedCircle = yCircle;
    }

    private void OnGrabZ(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedCircle = zCircle;
    }

    private void OnRelease(SelectExitEventArgs eventArgs)
    {
        selectedCircle = null;

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
    }
}
