using UnityEngine;

public class LightReveal : MonoBehaviour
{
    public float maxDistance = 20f;
    public LayerMask layerMask;

    private string colorProperty = "_Color"; // <- 
    private Light spotlight;

    void Start()
    {
        spotlight = GetComponent<Light>();
    }

    void Update()
    {
        if (spotlight == null)
            return;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, layerMask);

        foreach (RaycastHit hit in hits)
        {
            GameObject hitObject = hit.collider.gameObject;

            // Só altera se estiver na mesma layer da lanterna
            if (hitObject.layer != gameObject.layer)
                continue;

            Renderer renderer = hitObject.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Material mat = renderer.material;

            if (mat.HasProperty(colorProperty))
            {
                mat.SetColor(colorProperty, spotlight.color);
                Debug.Log($"Cor de '{hitObject.name}' alterada para {spotlight.color}");
            }
        }
    }
}
