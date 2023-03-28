using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public TextMeshProUGUI selectedObjectCountText;

    public static ObjectController Instance;
    List<GameObject> selectedGameobject = new();

    float scaleFactor = 0.5f;
    float positionFactor = 2f;
    float rotationFactor = 75f;

    bool scaleDecreaseButtonDown, scaleIncreaseButtonDown = false;

    bool xPositionIncreaseButtonDown, xPositionDecreaseButtonDown = false;
    bool yPositionIncreaseButtonDown, yPositionDecreaseButtonDown = false;
    bool zPositionIncreaseButtonDown, zPositionDecreaseButtonDown = false;

    bool xRotationIncreaseButtonDown, xRotationDecreaseButtonDown = false;
    bool yRotationIncreaseButtonDown, yRotationDecreaseButtonDown = false;
    bool zRotationIncreaseButtonDown, zRotationDecreaseButtonDown = false;

    public List<GameObject> SelectedGameobject { get => selectedGameobject; set => selectedGameobject = value; }
    public bool ScaleDecreaseButtonDown { get => scaleDecreaseButtonDown; set => scaleDecreaseButtonDown = value; }
    public bool ScaleIncreaseButtonDown { get => scaleIncreaseButtonDown; set => scaleIncreaseButtonDown = value; }
    public bool XPositionIncreaseButtonDown { get => xPositionIncreaseButtonDown; set => xPositionIncreaseButtonDown = value; }
    public bool XPositionDecreaseButtonDown { get => xPositionDecreaseButtonDown; set => xPositionDecreaseButtonDown = value; }
    public bool YPositionIncreaseButtonDown { get => yPositionIncreaseButtonDown; set => yPositionIncreaseButtonDown = value; }
    public bool YPositionDecreaseButtonDown { get => yPositionDecreaseButtonDown; set => yPositionDecreaseButtonDown = value; }
    public bool ZPositionIncreaseButtonDown { get => zPositionIncreaseButtonDown; set => zPositionIncreaseButtonDown = value; }
    public bool ZPositionDecreaseButtonDown { get => zPositionDecreaseButtonDown; set => zPositionDecreaseButtonDown = value; }
    public bool XRotationIncreaseButtonDown { get => xRotationIncreaseButtonDown; set => xRotationIncreaseButtonDown = value; }
    public bool XRotationDecreaseButtonDown { get => xRotationDecreaseButtonDown; set => xRotationDecreaseButtonDown = value; }
    public bool YRotationIncreaseButtonDown { get => yRotationIncreaseButtonDown; set => yRotationIncreaseButtonDown = value; }
    public bool YRotationDecreaseButtonDown { get => yRotationDecreaseButtonDown; set => yRotationDecreaseButtonDown = value; }
    public bool ZRotationIncreaseButtonDown { get => zRotationIncreaseButtonDown; set => zRotationIncreaseButtonDown = value; }
    public bool ZRotationDecreaseButtonDown { get => zRotationDecreaseButtonDown; set => zRotationDecreaseButtonDown = value; }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        selectedObjectCountText.text = "Selected object count: " + selectedGameobject.Count;
        HandleScale();
        HandlePosition();
        HandleRotation();
    }

    private void HandleRotation()
    {
        if (XRotationIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.Rotate(new Vector3(rotationFactor * Time.deltaTime, 0, 0));
            }
        }
        if (XRotationDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.Rotate(new Vector3(-rotationFactor * Time.deltaTime, 0, 0));
            }
        }

        if (YRotationIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.Rotate(new Vector3(0, rotationFactor * Time.deltaTime, 0));
            }
        }
        if (YRotationDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.Rotate(new Vector3(0, -rotationFactor * Time.deltaTime, 0));
            }
        }

        if (ZRotationIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.Rotate(new Vector3(0, 0, rotationFactor * Time.deltaTime));
            }
        }
        if (ZRotationDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.Rotate(new Vector3(0, 0, -rotationFactor * Time.deltaTime));
            }
        }
    }

    private void HandlePosition()
    {
        if (XPositionIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.position += new Vector3(positionFactor * Time.deltaTime, 0, 0);
            }
        }
        if (XPositionDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.position -= new Vector3(positionFactor * Time.deltaTime, 0, 0);
            }
        }

        if (YPositionIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.position += new Vector3(0, positionFactor * Time.deltaTime, 0);
            }
        }
        if (YPositionDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.position -= new Vector3(0, positionFactor * Time.deltaTime, 0);
            }
        }

        if (ZPositionIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.position += new Vector3(0, 0, positionFactor * Time.deltaTime);
            }
        }
        if (ZPositionDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.position -= new Vector3(0, 0, positionFactor * Time.deltaTime);
            }
        }
    }

    private void HandleScale()
    {
        if (ScaleIncreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                gameObject.transform.localScale += new Vector3(scaleFactor * Time.deltaTime, scaleFactor * Time.deltaTime, scaleFactor * Time.deltaTime);
            }
        }

        if (ScaleDecreaseButtonDown)
        {
            foreach (GameObject gameObject in selectedGameobject)
            {
                if (gameObject.transform.localScale.x > 0)
                    gameObject.transform.localScale -= new Vector3(scaleFactor * Time.deltaTime, scaleFactor * Time.deltaTime, scaleFactor * Time.deltaTime);
            }
        }
    }

    public void OnClickClearSelectedObjects()
    {
        foreach (GameObject gameObject in selectedGameobject)
        {
            gameObject.GetComponent<MeshController>().IsSelected = false;
            gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
        }

        selectedGameobject.Clear();
    }
}
