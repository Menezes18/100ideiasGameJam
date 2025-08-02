using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScene : MonoBehaviour
{
    public int index;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartGame();
        
    }
    
    void StartGame()
    {
        SceneTransitionManager.singleton.GoToSceneAsync(index);
    }
}
