using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    public void OnParticleCollision(GameObject other)
    {
        Debug.Log("Col");
    }
}
