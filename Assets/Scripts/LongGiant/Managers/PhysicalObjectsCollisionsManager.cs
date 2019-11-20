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
            newRequest.hitObject = hitObject;
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
        DebugTreatment(request);
        float hittingObjectAppliedForce = request.GetCollisionSpeed * request.hittingObject.GetObjectMass;
        float hitObjectAppliedForce = request.GetCollisionSpeed * request.hitObject.GetObjectMass;

        request.hittingObject.CheckForDestroy(hitObjectAppliedForce);
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
    public PhysicalObjectScript hitObject;
    public Vector3 relativeVelocity;
    public float GetCollisionSpeed { get { return relativeVelocity.magnitude; } }
}