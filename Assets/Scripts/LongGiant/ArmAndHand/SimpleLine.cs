using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HandPlacerUtility))]
[RequireComponent(typeof(LineRenderer))]
public class SimpleLine : MonoBehaviour
{
    LineRenderer lineRenderer;

    HandPlacerUtility handPlacer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;

        handPlacer = GetComponent<HandPlacerUtility>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        lineRenderer.SetPosition(0, Camera.main.transform.TransformPoint(handPlacer.cameraArmPlacement.start));
        lineRenderer.SetPosition(1, handPlacer.handRB.position);
    }
}
