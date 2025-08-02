using UnityEngine;

public class LightReveal : MonoBehaviour
{
    public float maxDistance = 20f;
    string colorProperty = "Color_70EBBDEE";  
    string erodeProperty = "_Erode";

    public LayerMask layerMask;
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
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer == null)
                continue;

            Material mat = renderer.material;
            

            bool hasColor = mat.HasProperty(colorProperty);
            bool hasErode = mat.HasProperty(erodeProperty);

            Debug.Log($"Objeto: {hit.collider.name}, Tem Cor? {hasColor}, Tem Erode? {hasErode}");

            if (hasColor)
            {
                Color objectColor = mat.GetColor(colorProperty);
                Color lightColor = spotlight.color;

                if (ColorsAreSimilar(objectColor, lightColor))
                {
                    mat.SetFloat(erodeProperty, 0f); 
                }
            }
        }
    }

    bool ColorsAreSimilar(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}
