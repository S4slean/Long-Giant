using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicalObjectScript : MonoBehaviour
{
    protected bool pendingDestroy;
    [Header("References")]
    [SerializeField] protected Rigidbody objectBody = default;

    [Header("Interactions Parameters")]
    [SerializeField] public PhysicalObjectInteractionsType physicalObjectInteractionsType = PhysicalObjectInteractionsType.DealAndReceiveDamages;

    public bool CanDealDamages { get { return physicalObjectInteractionsType == PhysicalObjectInteractionsType.OnlyDealDamages || physicalObjectInteractionsType == PhysicalObjectInteractionsType.DealAndReceiveDamages; } }
    public bool CanReceiveDamages { get { return physicalObjectInteractionsType == PhysicalObjectInteractionsType.OnlyReceiveDamages || physicalObjectInteractionsType == PhysicalObjectInteractionsType.DealAndReceiveDamages; } }

    [SerializeField] float minimumForceToBeDestroyed = 25;
    [SerializeField] float objectDamagingMass = 1;
    [SerializeField] float objectBodyMass= 1;
    public float GetObjectDamagingMass { get { return objectDamagingMass; } }

    [Header("Miscelaneous")]
    [SerializeField] float minimumRotationSpeedOnThrow = 180;
    [SerializeField] float maximumRotationSpeedOnThrow = 270;

    private ParticleSystem poofPS;

    bool setedUp;
    public virtual void SetUp()
    {
        if (setedUp)
            return;

        setedUp = true;

        if (objectBody == null)
            objectBody = GetComponent<Rigidbody>();

        poofPS = GetComponentInChildren<ParticleSystem>();

        objectBody.mass = objectBodyMass;
    }

    private void Start()
    {
        SetUp();
    }

    private void OnCollisionEnter(Collision collision)
    {
        /*PhysicalObjectScript hitPhysicalObject = collision.collider.GetComponent<PhysicalObjectScript>();
        if (hitPhysicalObject == null)*/
            PhysicalObjectScript hitPhysicalObject = collision.collider.GetComponentInParent<PhysicalObjectScript>();

        if (hitPhysicalObject != null)
            GameManager.gameManager.CollisionsManager.AskForCollisionTreatment(this, hitPhysicalObject, collision.relativeVelocity);
        else
            CheckForDestroy(collision.relativeVelocity.magnitude * objectDamagingMass);
    }

    public virtual bool CheckForDestroy(float speedForce)
    {
        if (!CanReceiveDamages)
            return false;

        if (pendingDestroy)
            return true;


        if (speedForce > minimumForceToBeDestroyed)
            pendingDestroy = true;

        return pendingDestroy;
    }

    public virtual void Update()
    {
        if (pendingDestroy)
            DestroyPhysicalObject();
    }

    public virtual void DestroyPhysicalObject()
    {
        poofPS.transform.parent = null;
        poofPS.Play();

        Destroy(gameObject);
    }

    public void Throw(Vector3 direction, float speed)
    {
        Throw(direction * speed);
    }

    public virtual void Throw(Vector3 velocity)
    {
        objectBody.AddForce(velocity, ForceMode.VelocityChange);
        objectBody.AddTorque(Random.onUnitSphere * Random.Range(minimumRotationSpeedOnThrow, maximumRotationSpeedOnThrow));
    }
}

public enum PhysicalObjectInteractionsType
{
    None,
    OnlyDealDamages,
    OnlyReceiveDamages,
    DealAndReceiveDamages
}
