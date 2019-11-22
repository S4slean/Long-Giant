using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowManager : MonoBehaviour
{
    public Camera shadowCamera;
    public static ShadowManager Instance;

    public Transform shadowPlane;

    private Camera main;

    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        shadowPlane.transform.position = new Vector3(main.transform.position.x, shadowPlane.transform.position.y, main.transform.position.z);

        shadowCamera.transform.position = new Vector3(main.transform.position.x, shadowPlane.transform.position.y + 10, main.transform.position.z);
    }

    public void Enable()
    {
        
    }

    public void Disable()
    {

    }
}
