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

public class HandController : MonoBehaviour
{
    public HandPlacerUtility handPlacer;


    public State state;

    private Vector3 endPosCache;

    private HandCollisionCallback handCallback;

    // Start is called before the first frame update
    void Start()
    {
        handCallback = GetComponentInChildren<HandCollisionCallback>();

        handCallback.OnCollision += OnHandCollision;
    }

    private void OnDisable()
    {
        handCallback.OnCollision -= OnHandCollision;
    }

    // Update is called once per frame
    void Update()
    {
        Touch touch;

        Debug.Log(Input.touches.Length);

        if (Input.touchCount != 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        RaycastHit hit;
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(touch.position);
        Debug.DrawRay(ray.origin, ray.direction);


        if (Physics.Raycast(ray, out hit))
        {
            Vector3 position = hit.point;

            handPlacer.joint.transform.position = position;

            handPlacer.placeJoint = false;
        }
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        Input.ResetTouches();
#endif
    }

    public void OnHandCollision(Collision col)
    {
        print("touched " + col.collider.name);
    }

    public enum State
    {
        Smash,
        Reaching,
        Grabbing
    }
}
