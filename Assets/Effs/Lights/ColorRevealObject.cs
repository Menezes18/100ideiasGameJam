using UnityEngine;

public class ColorRevealObject : MonoBehaviour
{
    [Header("Sai do Armario")]
    public Color revealColor = Color.white;
    [Range(0f, 1f)]
    public float colorTolerance = 0.1f;
    
    private bool isRevealed = false;
    private Material[] originalMaterials;
    private Material[] revealMaterials;
    
    void Start()
    {
    
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalMaterials = renderer.materials;
            revealMaterials = new Material[originalMaterials.Length];
            
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                revealMaterials[i] = new Material(originalMaterials[i]);
       
                SetMaterialTransparent(revealMaterials[i]);
            }
            
            renderer.materials = revealMaterials;
        }
    }
    
    void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3); 
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        
        Color color = mat.color;
        color.a = 0f;
        mat.color = color;
    }
    
    public void TryReveal(Color lightColor, float lightIntensity)
    {
        if (isRevealed) return;
        
        // Verifica se a cor da luz é compatível
        float colorDistance = Vector3.Distance(
            new Vector3(lightColor.r, lightColor.g, lightColor.b),
            new Vector3(revealColor.r, revealColor.g, revealColor.b)
        );
        
        if (colorDistance <= colorTolerance && lightIntensity > 0.5f)
        {
            RevealObject();
        }
    }
    
    void RevealObject()
    {
        if (isRevealed) return;
        
        isRevealed = true;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.materials = originalMaterials;
        }
    }
}