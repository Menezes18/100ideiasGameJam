using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(ActionBasedController))]
public class ContinuousRaycastWithGizmo : MonoBehaviour
{
    [Header("Origem do Raycast")]
    [Tooltip("Transform de onde o raio parte. Se ficar vazio, usa o próprio transform.")]
    public Transform rayOrigin;

    [Header("Configurações do Raycast")]
    [Tooltip("Distância máxima que o raio vai percorrer")]
    public float rayDistance = 10f;
    [Tooltip("Nome da layer que você quer detectar")]
    public string layerName = "SeuLayer";
    [Tooltip("Tag que você quer detectar")]
    public string targetTag = "Interactable";

    ActionBasedController actionController;

    void Awake()
    {
        actionController = GetComponent<ActionBasedController>();
        actionController.activateAction.action.Enable();
    }

    void Update()
    {
        Vector3 origin = rayOrigin != null ? rayOrigin.position : transform.position;
        Vector3 direction = rayOrigin != null ? rayOrigin.forward  : transform.forward;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance))
        {
            if (actionController.activateAction.action.triggered)
            {
                GameObject go = hit.collider.gameObject;

                if (go.CompareTag(targetTag))
                {
                    hit.collider.GetComponent<InteractableObject>()?.Interactable();
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = rayOrigin != null ? rayOrigin.position : transform.position;
        Vector3 direction = rayOrigin != null ? rayOrigin.forward  : transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, direction * rayDistance);
        Gizmos.DrawWireCube(origin + direction * rayDistance, Vector3.one * 0.05f);
    }
}
