using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple behavior that constantly looks at the camera location.
/// </summary>
public class BilboardScript : MonoBehaviour
{
    Camera mainCamera;
    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(mainCamera.transform.position);
    }
}
