using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A base class shared by all Physical Objects of the game (Resources, Constructions, Humans...)
/// Holds all references, variables, methods and behaviors required for handling physical collisions with other Physical Objects.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PhysicalObjectScript : MonoBehaviour
{
    protected bool pendingDestroy;
    [Header("References")]
    [SerializeField] protected Rigidbody objectBody = default;
    /// <summary>
    /// Defines how this object will react when hitting another one
    /// </summary>
    [Header("Interactions Parameters")]
    [Tooltip("Defines how this object will react when hitting another one.")]
    [SerializeField] public PhysicalObjectInteractionsType physicalObjectInteractionsType = PhysicalObjectInteractionsType.DealAndReceiveDamages;

    public bool CanDealDamages { get { return physicalObjectInteractionsType == PhysicalObjectInteractionsType.OnlyDealDamages || physicalObjectInteractionsType == PhysicalObjectInteractionsType.DealAndReceiveDamages; } }
    public bool CanReceiveDamages { get { return physicalObjectInteractionsType == PhysicalObjectInteractionsType.OnlyReceiveDamages || physicalObjectInteractionsType == PhysicalObjectInteractionsType.DealAndReceiveDamages; } }

    /// <summary>
    /// Defines how much the object is solid : The minimum velocity force (force + objectDamagingMass) this object should receive to get destroyed.
    /// </summary>
    [Tooltip("Defines how much the object is solid : The minimum velocity force (force + objectDamagingMass) this object should receive to get destroyed.")]
    [SerializeField] float minimumForceToBeDestroyed = 25;

    /// <summary>
    /// Defines how much the object is heavy : The velocity multiplier used to deal damages to another hit object (or to itseld if hit object is nit a Physical Object).
    /// </summary>
    [Tooltip("Defines how much the object is heavy : The velocity multiplier used to deal damages to another hit object (or to itseld if hit object is nit a Physical Object).")]
    [SerializeField] float objectDamagingMass = 1;

    /// <summary>
    /// Mass inputed in the basic Rigidbody behavior.
    /// </summary>
    [Tooltip("Mass inputed in the basic Rigidbody behavior")]
    [SerializeField] float objectBodyMass= 1;
    public float GetObjectDamagingMass { get { return objectDamagingMass; } }

    [Header("Miscelaneous")]
    [SerializeField] float minimumRotationSpeedOnThrow = 180;
    [SerializeField] float maximumRotationSpeedOnThrow = 270;
    [SerializeField] PlayRandomSound randomSound = default;

    private ParticleSystem poofPS;

    bool setedUp;


    #region Engine Callbacks

    private void Start()
    {
        SetUp();
    }

    public virtual void Update()
    {
        //Check if object is pending destroy, and destroy it if it is 
        if (pendingDestroy)
            DestroyPhysicalObject();
    }

    /// <summary>
    /// Handles collision with other Physical Objects, or other kind of objects
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        //Check if hit object is a PhysicalObject
        PhysicalObjectScript hitPhysicalObject = collision.collider.GetComponentInParent<PhysicalObjectScript>();

        //If it is, we ask for the Collision Manager to handle the collision between those two objects
        if (hitPhysicalObject != null)
            GameManager.gameManager.CollisionsManager.AskForCollisionTreatment(this, hitPhysicalObject, collision.relativeVelocity);
        //If it is not, we check if this object destroys under its own velocity and mass
        else
            CheckForDestroy(collision.relativeVelocity.magnitude * objectDamagingMass);
    }

    #endregion
    

    /// <summary>
    /// Initialize Physical Object references and behaviour
    /// </summary>
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


    #region Destroying

    /// <summary>
    /// We check if this object gets destroyed from the inputed speed force. If true, pending destroy will be set, and this object will get destroy on next Update.
    /// </summary>
    /// <param name="speedForce"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Destroys the object and plays some feedbacks
    /// </summary>
    public virtual void DestroyPhysicalObject()
    {
        if (poofPS != null)
        {
            poofPS.transform.parent = null;
            poofPS.Play();
        }

        if (randomSound != null)
            randomSound.PlaySound();

        Destroy(gameObject);
    }

    #endregion


    #region Throwing

    /// <summary>
    /// Throw the object with the inputed direction and speed
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="speed"></param>
    public void Throw(Vector3 direction, float speed)
    {
        Throw(direction * speed);
    }

    /// <summary>
    /// Throw the object with the inputed velocity
    /// </summary>
    /// <param name="velocity"></param>
    public virtual void Throw(Vector3 velocity)
    {
        objectBody.AddForce(velocity, ForceMode.VelocityChange);
        objectBody.AddTorque(Random.onUnitSphere * Random.Range(minimumRotationSpeedOnThrow, maximumRotationSpeedOnThrow));
    }

    #endregion
}

/// <summary>
/// Defines how the object will react when hitting another object
/// </summary>
public enum PhysicalObjectInteractionsType
{
    None,
    OnlyDealDamages,
    OnlyReceiveDamages,
    DealAndReceiveDamages
}
