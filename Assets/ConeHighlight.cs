// PaintIfHasScript.cs
using UnityEngine;
using System.Collections.Generic;

public class PaintIfHasScript : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 100f;

    [Header("Cor de highlight (pode ser trocada em tempo de execução)")]
    public Color hitColor = Color.red;
    
    [Header("Cone Settings")]
    public int rayCount = 8;
    public float coneAngle = 30f;
    public int rings = 3;
    public bool includeCenterRay = true;

    private List<GameObject> currentHits = new List<GameObject>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    // Método público para atualizar a cor de highlight
    public void SetHitColor(Color c)
    {
        hitColor = c;
    }

    void Update()
    {
        RestoreOriginalColors();
        currentHits.Clear();

        if (includeCenterRay)
            PerformRaycast(transform.forward);

        for (int ring = 1; ring <= rings; ring++)
        {
            float ringAngle = (coneAngle / rings) * ring;
            int raysInRing = Mathf.Max(1, rayCount * ring / rings);

            for (int i = 0; i < raysInRing; i++)
            {
                float angle = (360f / raysInRing) * i;
                Vector3 dir = GetConeRayDirection(ringAngle, angle);
                PerformRaycast(dir);
            }
        }
    }

    Vector3 GetConeRayDirection(float coneAngle, float rotationAngle)
    {
        Vector3 dir = new Vector3(
            Mathf.Sin(coneAngle * Mathf.Deg2Rad),
            0,
            Mathf.Cos(coneAngle * Mathf.Deg2Rad)
        );
        dir = Quaternion.AngleAxis(rotationAngle, Vector3.forward) * dir;
        return transform.TransformDirection(dir);
    }

    void PerformRaycast(Vector3 direction)
    {
        if (Physics.Raycast(transform.position, direction, out var hit, maxDistance))
        {
            var go = hit.collider.gameObject;
            if (go.GetComponent<HighlightScript>() != null && !currentHits.Contains(go))
            {
                var rend = go.GetComponent<Renderer>();
                if (rend && rend.material)
                {
                    if (!originalColors.ContainsKey(go))
                        originalColors[go] = rend.material.color;

                    rend.material.color = hitColor;
                    currentHits.Add(go);
                }
            }
        }
    }

    void RestoreOriginalColors()
    {
        foreach (var go in currentHits)
        {
            if (go && originalColors.TryGetValue(go, out var col))
            {
                var rend = go.GetComponent<Renderer>();
                if (rend && rend.material)
                    rend.material.color = col;
            }
        }
    }

    void OnDrawGizmos()
    {

        Gizmos.color = hitColor;
        
        // Desenhar raycast central
        if (includeCenterRay)
        {
            Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
        }

        // Desenhar raycasts do cone
        for (int ring = 1; ring <= rings; ring++)
        {
            float ringAngle = (coneAngle / rings) * ring;
            int raysInRing = Mathf.Max(1, rayCount * ring / rings);
            
            for (int i = 0; i < raysInRing; i++)
            {
                float angle = (360f / raysInRing) * i;
                Vector3 rayDirection = GetConeRayDirection(ringAngle, angle);
                Gizmos.DrawRay(transform.position, rayDirection * maxDistance);
            }
        }

        // Desenhar linha do contorno do cone (opcional)
        Gizmos.color = Color.yellow;
        for (int i = 0; i < 16; i++)
        {
            float angle = (360f / 16) * i;
            Vector3 coneEdge = GetConeRayDirection(coneAngle, angle);
            Gizmos.DrawRay(transform.position, coneEdge * maxDistance * 0.5f);
        }
    }

    void OnDestroy()
    {
        // Restaurar cores ao destruir o objeto
        RestoreOriginalColors();
    }
}