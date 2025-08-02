using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LogAButton : MonoBehaviour
{
    [Tooltip("Arraste aqui a InputAction 'Primary Button' (XRI RightHand)")]
    public InputActionReference primaryButton;

    [Tooltip("Arraste aqui o PaintIfHasScript que fará o highlight")]
    public PaintIfHasScript paintScript;

    [Header("Cores disponíveis")]
    public List<Color> colors = new List<Color> { Color.red, Color.green, Color.blue };
    
    private int currentColorIndex = 0;

    void OnEnable()
    {
        primaryButton.action.performed += _ => MudarCor();
        primaryButton.action.Enable();
    }

    void OnDisable()
    {
        primaryButton.action.performed -= _ => MudarCor();
        primaryButton.action.Disable();
    }

    private void MudarCor()
    {
        if (paintScript == null || colors.Count == 0)
            return;

        // Avança índice e faz wrap-around
        currentColorIndex = (currentColorIndex + 1) % colors.Count;
        Color novaCor = colors[currentColorIndex];

        // Passa a cor para o PaintIfHasScript
        paintScript.SetHitColor(novaCor);

        Debug.Log($"Nova cor de highlight: {novaCor}");
    }
}