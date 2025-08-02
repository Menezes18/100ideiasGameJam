using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    public Animator animator;
    
    public UnityEvent _InteractableEvent;
    public string _InteractableAnimationName = "Interact";

    private void Start()
    {
        if(animator != null)
            animator = GetComponent<Animator>();
    }

    public void Interactable()
    {
        animator.SetTrigger(_InteractableAnimationName);    
        _InteractableEvent?.Invoke();
    }
}
