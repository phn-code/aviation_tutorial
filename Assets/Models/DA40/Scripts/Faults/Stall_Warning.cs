using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Stall_Warning : Interact_Check
{
    [Header("Stall Warning Fault Parameters")]
    [SerializeField] private AudioSource audioSource;



    // Called when the user initiates an interaction with the part.
    public override void Interact()
    {
        base.Interact();

        audioSource.Play();
        Debug.Log("Stall Warning Play");
    }



    // Called when the user stops interacting with the part.
    public virtual void Release()
    {
        base.Release();

        audioSource.Stop();
    }
}
