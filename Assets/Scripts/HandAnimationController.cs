using UnityEngine;
using UnityEngine.InputSystem;

public class HandAnimationController : MonoBehaviour
{
    public InputActionProperty pincAction, gripAction;
    public Animator handAnimator;

    private void Update()
    {
        float pincActionValue = pincAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", pincActionValue);

        float gripActionValue = gripAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripActionValue);
    }
}
