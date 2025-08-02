using UnityEngine;

public class WaterParticleCollision : MonoBehaviour
{
    public ParticleSystem waterPS;

    public Growth _growth;
        
    void Start()
    {
        var col = waterPS.collision;
        col.enabled = true;
        col.sendCollisionMessages = true;
    }

    void OnParticleCollision(GameObject other)
    {
        if (other.tag == "Semente"){
            _growth.isGrowing = true;
            Debug.Log(other.name);
        }
        // if (other.CompareTag("Teste")){
        //     Debug.Log("Player collision");
        // }
    }
}