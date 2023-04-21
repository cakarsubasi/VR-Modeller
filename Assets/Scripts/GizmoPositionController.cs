using UnityEngine;

public class GizmoPositionController : MonoBehaviour
{
    public GameObject xArrow, yArrow, zArrow, xSquare, ySquare, zSquare;

    bool xArrowSelected, yArrowSelected, zArrowSelected;
    bool xSquareSelected, ySquareSelected, zSquareSelected;
    GameObject parent;


    public bool XArrowSelected { get => xArrowSelected; set => xArrowSelected = value; }
    public bool YArrowSelected { get => yArrowSelected; set => yArrowSelected = value; }
    public bool ZArrowSelected { get => zArrowSelected; set => zArrowSelected = value; }
    public bool XSquareSelected { get => xSquareSelected; set => xSquareSelected = value; }
    public bool YSquareSelected { get => ySquareSelected; set => ySquareSelected = value; }
    public bool ZSquareSelected { get => zSquareSelected; set => zSquareSelected = value; }

    private void Start()
    {
        parent = transform.parent.gameObject;
    }

    private void Update()
    {
        if (xArrowSelected)
        {
            yArrow.SetActive(false);
            zArrow.SetActive(false);
            xSquare.SetActive(false);
            ySquare.SetActive(false);
            zSquare.SetActive(false);
            parent.transform.position = xArrow.transform.position;
        }
        else if (yArrowSelected)
        {
            xArrow.SetActive(false);
            zArrow.SetActive(false);
            xSquare.SetActive(false);
            ySquare.SetActive(false);
            zSquare.SetActive(false);
            parent.transform.position = yArrow.transform.position;
        }
        else if (zArrowSelected)
        {
            xArrow.SetActive(false);
            yArrow.SetActive(false);
            xSquare.SetActive(false);
            ySquare.SetActive(false);
            zSquare.SetActive(false);
            parent.transform.position = zArrow.transform.position;
        }
        else if (xSquareSelected)
        {
            xArrow.SetActive(false);
            yArrow.SetActive(false);
            zArrow.SetActive(false);
            ySquare.SetActive(false);
            zSquare.SetActive(false);
            parent.transform.position = xSquare.transform.position;
        }
        else if (ySquareSelected)
        {
            xArrow.SetActive(false);
            yArrow.SetActive(false);
            zArrow.SetActive(false);
            xSquare.SetActive(false);
            zSquare.SetActive(false);
            parent.transform.position = ySquare.transform.position;
        }
        else if (zSquareSelected)
        {
            xArrow.SetActive(false);
            yArrow.SetActive(false);
            zArrow.SetActive(false);
            ySquare.SetActive(false);
            xSquare.SetActive(false);
            parent.transform.position = zSquare.transform.position;
        }
        else
        {
            xArrow.SetActive(true);
            yArrow.SetActive(true);
            zArrow.SetActive(true);
            xSquare.SetActive(true);
            ySquare.SetActive(true);
            zSquare.SetActive(true);

            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            float minValue = 0.25f;
            float maxValue = 1000f;
            float scaleFactor = 0.2f;
            transform.localScale = Vector3.one * Mathf.Clamp(distance * scaleFactor, minValue, maxValue);
        }
    }
}
