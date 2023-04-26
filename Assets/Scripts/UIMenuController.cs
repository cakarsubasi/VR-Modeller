using UnityEngine;
using UnityEngine.InputSystem;

public class UIMenuController : MonoBehaviour
{
    public InputActionReference primaryButton = null;
    public Material menuMat, selectMat, moveMat, scaleMat, rotateMat;
    public MeshRenderer selectMesh, moveMesh, scaleMesh, rotateMesh;

    private void Awake()
    {
        primaryButton.action.performed += UIMenu;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        primaryButton.action.performed -= UIMenu;
    }

    private void UIMenu(InputAction.CallbackContext context)
    {
        bool isActive = gameObject.activeSelf;
        gameObject.SetActive(!isActive);
    }

    public void SelectPressed()
    {
        if (ObjectController.Instance.Selecting)
        {
            selectMesh.material = menuMat;
            ObjectController.Instance.Selecting = false;
            ObjectController.Instance.OnSelect(false);
        }
        else
        {
            selectMesh.material = selectMat;
            ObjectController.Instance.Selecting = true;
            ObjectController.Instance.OnSelect(true);

            moveMesh.material = rotateMesh.material = scaleMesh.material = menuMat;
            ObjectController.Instance.Moving = ObjectController.Instance.Rotating = ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnMove(false);
            ObjectController.Instance.OnRotate(false);
        }
    }

    public void MovePressed()
    {
        if (ObjectController.Instance.Moving)
        {
            moveMesh.material = menuMat;
            ObjectController.Instance.Moving = false;
            ObjectController.Instance.OnMove(false);
        }
        else
        {
            moveMesh.material = moveMat;
            ObjectController.Instance.Moving = true;
            ObjectController.Instance.OnMove(true);

            selectMesh.material = rotateMesh.material = scaleMesh.material = menuMat;
            ObjectController.Instance.Selecting = ObjectController.Instance.Rotating = ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnSelect(false);
            ObjectController.Instance.OnRotate(false);
        }
    }

    public void RotatePressed()
    {
        if (ObjectController.Instance.Rotating)
        {
            rotateMesh.material = menuMat;
            ObjectController.Instance.Rotating = false;
            ObjectController.Instance.OnRotate(false);
        }
        else
        {
            rotateMesh.material = rotateMat;
            ObjectController.Instance.Rotating = true;
            ObjectController.Instance.OnRotate(true);

            moveMesh.material = selectMesh.material = scaleMesh.material = menuMat;
            ObjectController.Instance.Moving = ObjectController.Instance.Selecting = ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnSelect(false);
            ObjectController.Instance.OnMove(false);
        }
    }

    public void ScalePressed()
    {
        if (ObjectController.Instance.Scaling)
        {
            scaleMesh.material = menuMat;
            ObjectController.Instance.Scaling = false;
        }
        else
        {
            scaleMesh.material = scaleMat;
            ObjectController.Instance.Scaling = true;

            moveMesh.material = rotateMesh.material = selectMesh.material = menuMat;
            ObjectController.Instance.Moving = ObjectController.Instance.Rotating = ObjectController.Instance.Selecting = false;
            ObjectController.Instance.OnSelect(false);
            ObjectController.Instance.OnMove(false);
            ObjectController.Instance.OnRotate(false);
        }
    }
}
