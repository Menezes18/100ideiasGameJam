using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketStateMethods : MonoBehaviour
{
    public XRSocketInteractor socket;
    bool wasConnected;
    public Animator animator;
    public UnityEvent onConnected;
    public UnityEvent onDisconnected;
    void Awake()
    {
        wasConnected = socket.hasSelection;
    }
    
    void Update()
    {
        bool isConnected = socket.hasSelection;
        if (isConnected != wasConnected)
        {
            wasConnected = isConnected;
            if (isConnected)
                OnSocketConnected(socket.firstInteractableSelected.transform.gameObject);
            else
                OnSocketDisconnected();
        }
    }
    void OnSocketConnected(GameObject obj)
    {
        if(animator != null) animator.SetTrigger("Open");
        onConnected?.Invoke();
        Debug.Log($"🔌 Conectado: {obj.name}");
    }

  
    void OnSocketDisconnected()
    {
        if(animator != null) animator.SetTrigger("Close");
        onDisconnected?.Invoke();
        Debug.Log("⚠️ Desconectado: socket ficou vazio");
    }
}