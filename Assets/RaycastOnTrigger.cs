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
        // certifica-se de que a ação de activate está habilitada
        actionController.activateAction.action.Enable();
    }

    void Update()
    {
        // escolhe origem e direção do raio
        Vector3 origin    = rayOrigin != null ? rayOrigin.position : transform.position;
        Vector3 direction = rayOrigin != null ? rayOrigin.forward  : transform.forward;

        // dispara o raycast a cada frame
        if (Physics.Raycast(origin, direction, out RaycastHit hit, rayDistance))
        {
            if (actionController.activateAction.action.triggered)
            {
                GameObject go = hit.collider.gameObject;

                if (go.layer == LayerMask.NameToLayer(layerName))
                    Debug.Log($"[Raycast Update] Hit na layer “{layerName}”: {go.name}");

                if (go.CompareTag(targetTag))
                    Debug.Log($"[Raycast Update] Hit na tag “{targetTag}”: {go.name}");
            }
        }
    }

    // desenha o gizmo no editor
    void OnDrawGizmosSelected()
    {
        Vector3 origin = rayOrigin != null ? rayOrigin.position : transform.position;
        Vector3 direction = rayOrigin != null ? rayOrigin.forward  : transform.forward;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(origin, direction * rayDistance);
        Gizmos.DrawWireCube(origin + direction * rayDistance, Vector3.one * 0.05f);
    }
}
