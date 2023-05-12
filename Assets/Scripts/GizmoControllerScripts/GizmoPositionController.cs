using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GizmoPositionController : MonoBehaviour
{
    Transform targetObject;

    public GameObject xArrow, yArrow, zArrow, xSquare, ySquare, zSquare;

    public float moveSpeed;

    GameObject selectedHandle;
    Vector3 previousPosition;


    private void Start()
    {
        targetObject = transform.parent.gameObject.transform;

        xArrow.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabXArrow);
        xArrow.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        yArrow.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabYArrow);
        yArrow.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        zArrow.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabZArrow);
        zArrow.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        xSquare.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabXSquare);
        xSquare.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        ySquare.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabYSquare);
        ySquare.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);

        zSquare.GetComponent<XRSimpleInteractable>().selectEntered.AddListener(OnGrabZSquare);
        zSquare.GetComponent<XRSimpleInteractable>().selectExited.AddListener(OnRelease);
    }

    private void Update()
    {
        if (selectedHandle != null)
        {
            Vector3 controllerPosition = selectedHandle.GetComponent<XRSimpleInteractable>().firstInteractorSelecting.transform.position;
            Vector3 controllerDelta = controllerPosition - previousPosition;

            if (selectedHandle == xArrow)
            {
                float moveDelta = controllerDelta.x * moveSpeed;
                targetObject.position = new Vector3(targetObject.position.x + moveDelta, targetObject.position.y, targetObject.position.z);
            }
            else if (selectedHandle == yArrow)
            {
                float moveDelta = controllerDelta.y * moveSpeed;
                targetObject.position = new Vector3(targetObject.position.x, targetObject.position.y + moveDelta, targetObject.position.z);
            }
            else if (selectedHandle == zArrow)
            {
                float moveDelta = controllerDelta.z * moveSpeed;
                targetObject.position = new Vector3(targetObject.position.x, targetObject.position.y, targetObject.position.z + moveDelta);
            }
            else if (selectedHandle == xSquare)
            {
                float moveDeltaY = controllerDelta.y * moveSpeed;
                float moveDeltaZ = controllerDelta.z * moveSpeed;
                targetObject.position = new Vector3(targetObject.position.x, targetObject.position.y + moveDeltaY, targetObject.position.z + moveDeltaZ);
            }
            else if (selectedHandle == ySquare)
            {
                float moveDeltaX = controllerDelta.x * moveSpeed;
                float moveDeltaZ = controllerDelta.z * moveSpeed;
                targetObject.position = new Vector3(targetObject.position.x + moveDeltaX, targetObject.position.y, targetObject.position.z + moveDeltaZ);
            }
            else if (selectedHandle == zSquare)
            {
                float moveDeltaX = controllerDelta.x * moveSpeed;
                float moveDeltaY = controllerDelta.y * moveSpeed;
                targetObject.position = new Vector3(targetObject.position.x + moveDeltaX, targetObject.position.y + moveDeltaY, targetObject.position.z);
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

    private void OnGrabXArrow(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = xArrow;

        yArrow.SetActive(false);
        zArrow.SetActive(false);
        xSquare.SetActive(false);
        ySquare.SetActive(false);
        zSquare.SetActive(false);
    }

    private void OnGrabYArrow(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = yArrow;

        xArrow.SetActive(false);
        zArrow.SetActive(false);
        xSquare.SetActive(false);
        ySquare.SetActive(false);
        zSquare.SetActive(false);
    }

    private void OnGrabZArrow(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = zArrow;

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        xSquare.SetActive(false);
        ySquare.SetActive(false);
        zSquare.SetActive(false);
    }

    private void OnGrabXSquare(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = xSquare;

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        zArrow.SetActive(false);
        ySquare.SetActive(false);
        zSquare.SetActive(false);
    }

    private void OnGrabYSquare(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = ySquare;

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        zArrow.SetActive(false);
        xSquare.SetActive(false);
        zSquare.SetActive(false);
    }

    private void OnGrabZSquare(SelectEnterEventArgs eventArgs)
    {
        previousPosition = eventArgs.interactorObject.transform.position;
        selectedHandle = zSquare;

        xArrow.SetActive(false);
        yArrow.SetActive(false);
        zArrow.SetActive(false);
        ySquare.SetActive(false);
        xSquare.SetActive(false);
    }

    private void OnRelease(SelectExitEventArgs eventArgs)
    {
        selectedHandle = null;

        xArrow.SetActive(true);
        yArrow.SetActive(true);
        zArrow.SetActive(true);
        xSquare.SetActive(true);
        ySquare.SetActive(true);
        zSquare.SetActive(true);
    }
}
