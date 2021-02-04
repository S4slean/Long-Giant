using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class that manages collisions occuring between two Physical Objects.
/// </summary>
[System.Serializable]
public class PhysicalObjectsCollisionsManager
{
    List<PhysicalObjectsCollisionRequest> occuringCollisionsRequests = new List<PhysicalObjectsCollisionRequest>();

    /// <summary>
    /// Gets a collision between two Physical Objects and generates a Collision Request that will be treated on Late Update.
    /// </summary>
    /// <param name="hittingObject">The Physical Object responsible for the Collision Callback</param>
    /// <param name="hitObject">The Physical Object that got hit</param>
    /// <param name="relativeVelocity">The relative velocity with which objects collided</param>
    public void AskForCollisionTreatment(PhysicalObjectScript hittingObject, PhysicalObjectScript hitObject, Vector3 relativeVelocity)
    {
        bool alreadyTreated = false;

        //Check if objects are not null
        if(hittingObject == null || hitObject == null)
        {
            Debug.LogError("ASKING FOR IMPOSSIBLE COLLISION : No collision treated");
            return;
        }

        //Check if this collision hasn't been already triggered previously on this update, by searching the hittinh object amoung the already requested hit objects.
        for(int i = 0; i < occuringCollisionsRequests.Count; i++)
        {
            PhysicalObjectsCollisionRequest request = occuringCollisionsRequests[i];
            if(request.hitObject == hittingObject)
            {
                alreadyTreated = true;
                break;
            }
        }

        //Create and add the request if it wasn't treated yet.
        if (!alreadyTreated)
        {
            PhysicalObjectsCollisionRequest newRequest = new PhysicalObjectsCollisionRequest();
            newRequest.hittingObject = hittingObject;
            newRequest.hittingObjectMass = hittingObject.CanDealDamages ? hittingObject.GetObjectDamagingMass : 0;
            newRequest.hitObject = hitObject;
            newRequest.hitObjectMass = hitObject.CanDealDamages ? hitObject.GetObjectDamagingMass : 0;
            newRequest.relativeVelocity = relativeVelocity;
            occuringCollisionsRequests.Add(newRequest);
        }
    }

    /// <summary>
    /// Treat every pending requests and clear requests
    /// </summary>
    public void TreatRequests()
    {
        foreach (PhysicalObjectsCollisionRequest request in occuringCollisionsRequests)
            TreatSingleRequest(request);

        occuringCollisionsRequests = new List<PhysicalObjectsCollisionRequest>();
    }

    /// <summary>
    /// Treat a single collision request, by calling the CheckForDestruction method on each object, with the ObjectMass of the other object. 
    /// </summary>
    /// <param name="request"></param>
    public void TreatSingleRequest(PhysicalObjectsCollisionRequest request)
    {
        float hittingObjectAppliedForce = request.GetCollisionSpeed * request.hittingObjectMass;
        float hitObjectAppliedForce = request.GetCollisionSpeed * request.hitObjectMass;

        //Damaging force applied by the hit object is sent to the hitting object
        if (request.hittingObject != null)
            request.hittingObject.CheckForDestroy(hitObjectAppliedForce);

        //Damaging force applied by the hitting object is sent to the hit object
        if (request.hitObject != null)
            request.hitObject.CheckForDestroy(hittingObjectAppliedForce);
    }
}

/// <summary>
/// Defines a request generated when two Physical Objects collides with each other
/// </summary>
public struct PhysicalObjectsCollisionRequest
{
    public PhysicalObjectScript hittingObject;
    public float hittingObjectMass;
    public PhysicalObjectScript hitObject;
    public float hitObjectMass;
    public Vector3 relativeVelocity;
    public float GetCollisionSpeed { get { return relativeVelocity.magnitude; } }
}