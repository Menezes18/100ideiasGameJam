using UnityEngine;
using UnityEngine.InputSystem;

public class LogAButton : MonoBehaviour
{
    [Tooltip("Arraste aqui a InputAction 'Primary Button' (XRI RightHand)")]
    public InputActionReference primaryButton;
    public bool handActivated = false;
    void OnEnable()
    {
        primaryButton.action.performed += _ => MudarCor();
        primaryButton.action.Enable();
    }

    public void MudarCor()
    {
        if (handActivated){
            Debug.Log("is hand activated");
        }
        else{
            Debug.Log("is not hand activated");
        }
    }

    public void PodeMudarCor(bool activated)
    {
        handActivated = activated;
    }
    void OnDisable()
    {
        // primaryButton.action.performed -= _ => Debug.Log("A pressionado!");
        // primaryButton.action.Disable();
    }
}