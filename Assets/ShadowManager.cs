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

        main = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //shadowPlane.transform.position = new Vector3(GameManager.gameManager.GetGiantConstruction.transform.position.x, shadowPlane.transform.position.y, shadowPlane.transform.position.z);

        //shadowCamera.transform.position = new Vector3(shadowPlane.transform.position.x, shadowPlane.transform.position.y + 10, shadowPlane.transform.position.z);
    }

    public void Enable()
    {
        shadowPlane.gameObject.SetActive(true);

        shadowPlane.position = new Vector3(GameManager.gameManager.GetGiantConstruction.transform.position.x, GameManager.gameManager.GetGiantConstruction.transform.position.y, GameManager.gameManager.GetGiantConstruction.transform.position.z);
        shadowCamera.transform.position = new Vector3(shadowPlane.transform.position.x, shadowPlane.transform.position.y + 10, shadowPlane.transform.position.z);
    }

    public void Disable()
    {
        shadowPlane.gameObject.SetActive(false);
    }
}
