using UnityEngine;

public class RaycastFromTransform : MonoBehaviour
{
    [Header("Origem do Raycast")]
    [Tooltip("Transform de onde o raio será disparado")]
    public Transform originTransform;

    [Header("Parâmetros do Raycast")]
    [Tooltip("Distância máxima do raycast")]
    public float maxDistance = 10f;
    [Tooltip("Tag que deve ser detectada")]
    public string targetTag = "TargetTag";

    [Header("Direção")]
    [Tooltip("Direção do raycast em relação ao Transform de origem")]
    public Vector3 directionLocal = Vector3.forward;

    void Update()
    {
        if (originTransform == null)
        {
            Debug.LogWarning("Origin Transform não definido!", this);
            return;
        }

        Vector3 origin = originTransform.position;
        Vector3 direction = originTransform.TransformDirection(directionLocal.normalized);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                Debug.Log($"Raycast partindo de '{originTransform.name}' atingiu '{hit.collider.name}' com tag '{targetTag}'");
            }
        }

        Debug.DrawRay(origin, direction * maxDistance, Color.green);
    }
}