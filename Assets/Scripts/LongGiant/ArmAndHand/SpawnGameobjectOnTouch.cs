using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.Examples.Common;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class SpawnGameobjectOnTouch : MonoBehaviour
{
    public GameObject go;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Touch touch;
        if (Input.touchCount < 2 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        touch = Input.GetTouch(0);

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        RaycastHit hit;
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out hit))
        {
            GameObject newGo = Instantiate(go);

            Vector3 position = hit.point;

            newGo.transform.position = position;
        }
    }

}
