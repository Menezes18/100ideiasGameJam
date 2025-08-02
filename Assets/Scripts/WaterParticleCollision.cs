using UnityEngine;

public class WaterParticleCollision : MonoBehaviour
{
    public ParticleSystem waterPS;

    public float growthIncrement = 0.1f;

    void Start()
    {
        var col = waterPS.collision;
        col.enabled = true;
        col.sendCollisionMessages = true;
    }

    void OnParticleCollision(GameObject other)
    {
        Debug.Log("Player collision " + other.tag);
        // if (other.CompareTag("Teste")){
        //     Debug.Log("Player collision");
        // }
    }
}