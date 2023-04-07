using UnityEngine;

public class GizmoPositionArrowController : MonoBehaviour
{
    public GameObject xArrow, yArrow, zArrow;

    bool xArrowSelected, yArrowSelected, zArrowSelected;
    GameObject parent;


    public bool XArrowSelected { get => xArrowSelected; set => xArrowSelected = value; }
    public bool YArrowSelected { get => yArrowSelected; set => yArrowSelected = value; }
    public bool ZArrowSelected { get => zArrowSelected; set => zArrowSelected = value; }
}
