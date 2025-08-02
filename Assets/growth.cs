using UnityEngine;

public class Growth : MonoBehaviour
{
    [Header("Configuração de escala")]
    [Tooltip("Escala alvo que o objeto deve alcançar")]
    public Vector3 targetScale = new Vector3(2f, 2f, 2f);

    [Tooltip("Duração da interpolação, em segundos")]
    public float duration = 3f;

    // Variáveis internas
    public Vector3 startScale;
    private float timer;
    public bool isGrowing;

    void Update()
    {
        if (!isGrowing) 
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / duration);

        transform.localScale = Vector3.Lerp(startScale, targetScale, t);
        
    }


    public void BeginGrowth()
    {
        startScale = transform.localScale;

        timer = 0f;

        isGrowing = true;
    }
}