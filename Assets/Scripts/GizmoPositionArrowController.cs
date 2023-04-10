using UnityEngine;

public class GizmoPositionArrowController : MonoBehaviour
{
    public GameObject xArrow, yArrow, zArrow, xSquare, ySquare, zSquare;

    bool xArrowSelected, yArrowSelected, zArrowSelected;
    GameObject parent;


    public bool XArrowSelected { get => xArrowSelected; set => xArrowSelected = value; }
    public bool YArrowSelected { get => yArrowSelected; set => yArrowSelected = value; }
    public bool ZArrowSelected { get => zArrowSelected; set => zArrowSelected = value; }

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
        else
        {
            xArrow.SetActive(true);
            yArrow.SetActive(true);
            zArrow.SetActive(true);
            xSquare.SetActive(true);
            ySquare.SetActive(true);
            zSquare.SetActive(true);
        }
    }
}
