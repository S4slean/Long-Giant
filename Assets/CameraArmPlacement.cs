using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArmPlacement : MonoBehaviour
{
    public Camera cam;

    public bool debug;

    public Vector3 start;
    public Vector3 end;

    WigglyLine wigglyLine;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(cam.transform.TransformPoint(start),.01f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(cam.transform.TransformPoint(end), .01f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(cam.transform.TransformPoint(start), cam.transform.TransformPoint(end));
    }
}
