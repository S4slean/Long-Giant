using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WigglyLine))]
public class CameraArmPlacement : MonoBehaviour
{
    public bool debug;

    public Vector3 start;
    public Vector3 end;

    WigglyLine wigglyLine;

    // Start is called before the first frame update
    void Start()
    {
        wigglyLine = GetComponent<WigglyLine>();
    }

    // Update is called once per frame
    void Update()
    {
        wigglyLine.start = Camera.main.transform.TransformPoint(start);
        wigglyLine.end = Camera.main.transform.TransformPoint(end);
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(Camera.main.transform.TransformPoint(start),.05f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(Camera.main.transform.TransformPoint(end), .05f);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(Camera.main.transform.TransformPoint(start), Camera.main.transform.TransformPoint(end));
    }
}
