using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(WigglyLine))]
public class LineDebug : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private WigglyLine wigglyLine;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        wigglyLine = GetComponent<WigglyLine>();

        lineRenderer.positionCount = wigglyLine.subdiv;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPositions(wigglyLine.positions);
    }
}
