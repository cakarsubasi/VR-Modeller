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
            ObjectController.Instance.OnSelect();
        }
        else
        {
            selectMesh.material = selectMat;
            ObjectController.Instance.Selecting = true;
            ObjectController.Instance.OnSelect();

            moveMesh.material = rotateMesh.material = scaleMesh.material = menuMat;
            ObjectController.Instance.Moving = ObjectController.Instance.Rotating = ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnMove();
            ObjectController.Instance.OnRotate();
            ObjectController.Instance.OnScale();
        }
    }

    public void MovePressed()
    {
        if (ObjectController.Instance.Moving)
        {
            moveMesh.material = menuMat;
            ObjectController.Instance.Moving = false;
            ObjectController.Instance.OnMove();
        }
        else
        {
            moveMesh.material = moveMat;
            ObjectController.Instance.Moving = true;
            ObjectController.Instance.OnMove();

            selectMesh.material = rotateMesh.material = scaleMesh.material = menuMat;
            ObjectController.Instance.Selecting = ObjectController.Instance.Rotating = ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnSelect();
            ObjectController.Instance.OnRotate();
            ObjectController.Instance.OnScale();
        }
    }

    public void RotatePressed()
    {
        if (ObjectController.Instance.Rotating)
        {
            rotateMesh.material = menuMat;
            ObjectController.Instance.Rotating = false;
            ObjectController.Instance.OnRotate();
        }
        else
        {
            rotateMesh.material = rotateMat;
            ObjectController.Instance.Rotating = true;
            ObjectController.Instance.OnRotate();

            moveMesh.material = selectMesh.material = scaleMesh.material = menuMat;
            ObjectController.Instance.Moving = ObjectController.Instance.Selecting = ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnSelect();
            ObjectController.Instance.OnMove();
            ObjectController.Instance.OnScale();
        }
    }

    public void ScalePressed()
    {
        if (ObjectController.Instance.Scaling)
        {
            scaleMesh.material = menuMat;
            ObjectController.Instance.Scaling = false;
            ObjectController.Instance.OnScale();
        }
        else
        {
            scaleMesh.material = scaleMat;
            ObjectController.Instance.Scaling = true;
            ObjectController.Instance.OnScale();

            moveMesh.material = rotateMesh.material = selectMesh.material = menuMat;
            ObjectController.Instance.Moving = ObjectController.Instance.Rotating = ObjectController.Instance.Selecting = false;
            ObjectController.Instance.OnSelect();
            ObjectController.Instance.OnMove();
            ObjectController.Instance.OnRotate();
        }
    }
}
