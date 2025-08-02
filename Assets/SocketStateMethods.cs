using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/Custom Socket Interactor")]
public class TaggedSocketWithState : XRSocketInteractor
{
    [Header("Tag Permitida")]
    [Tooltip("Só permite selecionar objetos com esta Tag")]
    [SerializeField] string allowedTag = "Permitido";

    [Header("Estado do Socket")]
    [Tooltip("Animator para abrir/fechar porta")]
    public Animator animator;
    public UnityEvent onConnected;
    public UnityEvent onDisconnected;

    bool wasConnected;

    protected override void Awake()
    {
        base.Awake();
        wasConnected = hasSelection;
    }

    void Update()
    {
        // Monitora mudança de estado de conexão
        bool isConnected = hasSelection;
        if (isConnected != wasConnected)
        {
            wasConnected = isConnected;
            if (isConnected)
                OnSocketConnected(firstInteractableSelected.transform.gameObject);
            else
                OnSocketDisconnected();
        }
    }

    void OnSocketConnected(GameObject obj)
    {
        animator?.SetTrigger("Open");
        onConnected?.Invoke();
        Debug.Log($"🔌 Conectado: {obj.name}");
    }

    void OnSocketDisconnected()
    {
        animator?.SetTrigger("Close");
        onDisconnected?.Invoke();
        Debug.Log("⚠️ Desconectado: socket ficou vazio");
    }
    
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        var go = (interactable as Component)?.gameObject;
        if (go == null || !go.CompareTag(allowedTag))
            return false;

        // Mantém toda a lógica padrão de seleção do socket
        return base.CanSelect(interactable) &&
               ((!hasSelection && !interactable.isSelected) ||
                (IsSelecting(interactable) && interactable.interactorsSelecting.Count == 1));
    }
}