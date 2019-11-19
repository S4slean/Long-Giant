using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicalObjectScript : MonoBehaviour
{
    [Header("Interactions Parameters")]
    [SerializeField] PhysicalObjectInteractionsType physicalObjectInteractionsType = PhysicalObjectInteractionsType.DealAndReceiveDamages;
    [SerializeField] float minimumSpeedToBeDestroyed = 25;
    [SerializeField] bool canCurrentlyDealDamages = false;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.relativeVelocity);
    }

    public bool CheckForDestroy(float speedForce)
    {
        bool destroyed = false;

        if(speedForce > minimumSpeedToBeDestroyed)
        {
            destroyed = true;
            DestroyObject();
        }

        return destroyed;
    }

    public virtual void DestroyObject()
    {
        Debug.Log(name + " destroyed because of speed");
    }
}

public enum PhysicalObjectInteractionsType
{
    None,
    OnlyDealDamages,
    OnlyReceiveDamages,
    DealAndReceiveDamages
}
