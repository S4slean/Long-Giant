using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicalObjectScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody objectBody = default;

    [Header("Interactions Parameters")]
    [SerializeField] PhysicalObjectInteractionsType physicalObjectInteractionsType = PhysicalObjectInteractionsType.DealAndReceiveDamages;
    [SerializeField] float minimumForceToBeDestroyed = 25;
    [SerializeField] float objectMass = 1;
    public float GetObjectMass { get { return objectMass; } }

    bool setedUp;
    public void SetUp()
    {
        if (setedUp)
            return;

        setedUp = true;

        if (objectBody == null)
            objectBody = GetComponent<Rigidbody>();

        objectBody.mass = objectMass;
    }

    private void Start()
    {
        SetUp();
    }

    private void OnCollisionEnter(Collision collision)
    {
        PhysicalObjectScript hitPhysicalObject = collision.collider.GetComponent<PhysicalObjectScript>();
        if (hitPhysicalObject != null)
            GameManager.gameManager.CollisionsManager.AskForCollisionTreatment(this, hitPhysicalObject, collision.relativeVelocity);
        else
            CheckForDestroy(collision.relativeVelocity.magnitude * objectMass);
    }

    public bool CheckForDestroy(float speedForce)
    {
        bool destroyed = false;

        Debug.Log(name + " receives force of " + speedForce);

        if(speedForce > minimumForceToBeDestroyed)
        {
            destroyed = true;
            DestroyObject();
        }

        return destroyed;
    }

    public virtual void DestroyObject()
    {
        Debug.Log(name + " destroyed because of speed");
        Destroy(gameObject);
    }
}

public enum PhysicalObjectInteractionsType
{
    None,
    OnlyDealDamages,
    OnlyReceiveDamages,
    DealAndReceiveDamages
}
