using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPlacerUtility : MonoBehaviour
{
    public Camera cam;

    public CameraArmPlacement cameraArmPlacement;

    public Rigidbody handRB;
    public Joint joint;

    private void Update()
    {
        joint.transform.position = cam.transform.TransformPoint(cameraArmPlacement.end);
        joint.transform.rotation = cam.transform.rotation;
    }

    [ContextMenu("Update")]
    public void UpdateHandPlacement()
    {
        joint.transform.position = cam.transform.TransformPoint(cameraArmPlacement.end);
        handRB.transform.position = joint.transform.position += Vector3.forward * handRB.transform.localScale.x*.5f;
    }
}
