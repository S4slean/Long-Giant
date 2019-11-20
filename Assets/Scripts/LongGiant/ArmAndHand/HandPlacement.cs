using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WigglyLine))]
public class HandPlacement : MonoBehaviour
{
    public Transform handTransform;

    private WigglyLine wigglyLine;

    // Start is called before the first frame update
    void Start()
    {
        wigglyLine = GetComponent<WigglyLine>();
    }

    // Update is called once per frame
    void Update()
    {
        handTransform.position = wigglyLine.positions[wigglyLine.subdiv - 1];
    }
}
