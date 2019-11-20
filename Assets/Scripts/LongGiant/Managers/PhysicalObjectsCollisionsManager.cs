using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhysicalObjectsCollisionsManager
{
    List<PhysicalObjectsCollisionRequest> occuringCollisionsRequests = new List<PhysicalObjectsCollisionRequest>();

    public void AskForCollisionTreatment(PhysicalObjectScript hittingObject, PhysicalObjectScript hitObject, Vector3 relativeVelocity)
    {
        bool alreadyTreated = false;

        if(hittingObject == null || hitObject == null)
        {
            Debug.LogError("ASKING FOR IMPOSSIBLE COLLISION : No collision treated");
            return;
        }

        for(int i = 0; i < occuringCollisionsRequests.Count; i++)
        {
            PhysicalObjectsCollisionRequest request = occuringCollisionsRequests[i];
            if(request.hitObject == hittingObject)
            {
                alreadyTreated = true;
                break;
            }
        }

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

    public void TreatRequests()
    {
        foreach (PhysicalObjectsCollisionRequest request in occuringCollisionsRequests)
            TreatSingleRequest(request);

        occuringCollisionsRequests = new List<PhysicalObjectsCollisionRequest>();
    }

    public void TreatSingleRequest(PhysicalObjectsCollisionRequest request)
    {
        float hittingObjectAppliedForce = request.GetCollisionSpeed * request.hittingObjectMass;
        float hitObjectAppliedForce = request.GetCollisionSpeed * request.hitObjectMass;

        if (request.hittingObject != null)
            request.hittingObject.CheckForDestroy(hitObjectAppliedForce);

        if (request.hitObject != null)
            request.hitObject.CheckForDestroy(hittingObjectAppliedForce);
    }

    public void DebugTreatment(PhysicalObjectsCollisionRequest request)
    {
        Debug.Log("Treating " + request.hittingObject.name + " hitting " + request.hitObject.name + " at speed " + request.GetCollisionSpeed);
    }
}

public struct PhysicalObjectsCollisionRequest
{
    public PhysicalObjectScript hittingObject;
    public float hittingObjectMass;
    public PhysicalObjectScript hitObject;
    public float hitObjectMass;
    public Vector3 relativeVelocity;
    public float GetCollisionSpeed { get { return relativeVelocity.magnitude; } }
}