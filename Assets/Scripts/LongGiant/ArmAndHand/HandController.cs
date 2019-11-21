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
    public float throwForce;
    public GameObject openHand;
    public GameObject closedhand;
    public LayerMask layerMask;

    public State state;

    private Vector3 endPosCache;

    private HandCollisionCallback handCallback;
    private HandPlacerUtility handPlacer;
    private PhysicalObjectScript handPhysicalObject;
    private Rigidbody handBody;
    public ConfigurableJoint joint;

    private PhysicalObjectScript grabbedObj;

    // Start is called before the first frame update
    void Start()
    {
        handPlacer = GetComponentInChildren<HandPlacerUtility>();
        handCallback = GetComponentInChildren<HandCollisionCallback>();
        handPhysicalObject = GetComponentInChildren<PhysicalObjectScript>();
        handBody = GetComponentInChildren<Rigidbody>();

        handCallback.OnCollision += OnHandCollision;
    }

    private void OnDisable()
    {
        handCallback.OnCollision -= OnHandCollision;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.Grabbing)
        {
            // Grabbed object has been destroyed
            if (grabbedObj == null)
            {
                state = State.Smash;
                SetJointActive(false);
                OpenHand(true);
            }
        }

        Touch touch;

        if (Input.touchCount != 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        if (state == State.Reaching)
        {
            state = State.Smash;
            handPlacer.placeJoint = true;

            return;
        }

        if (state == State.Smash)
        {
            RaycastHit hit;
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(touch.position);
            Debug.DrawRay(ray.origin, ray.direction);


            if (Physics.Raycast(ray, out hit, layerMask.value))
            {
                Vector3 position = hit.point;

                handPlacer.joint.transform.position = position;
                handPlacer.placeJoint = false;

                handPhysicalObject.physicalObjectInteractionsType = PhysicalObjectInteractionsType.None;

                state = State.Reaching;
            }

            return;
        }

        if (state == State.Grabbing)
        {
            Camera cam = Camera.main;
            Ray ray = cam.ScreenPointToRay(touch.position);

            SetJointActive(false);

            grabbedObj.Throw(ray.direction, throwForce);
            grabbedObj = null;

            OpenHand(true);

            return;
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
        if (state == State.Reaching)
        {
            PhysicalObjectScript physObj = col.collider.GetComponent<PhysicalObjectScript>();

            if (physObj != null)
            {
                grabbedObj = physObj;

                Rigidbody objRB = physObj.GetComponent<Rigidbody>();

                objRB.transform.position = handBody.transform.TransformPoint(objRB.GetComponent<Collider>().bounds.extents);

                joint.connectedBody = physObj.GetComponent<Rigidbody>();
                SetJointActive(true);

                state = State.Grabbing;

                OpenHand(false);
            }
            else
            {
                state = State.Smash;
            }

            handPlacer.placeJoint = true;
        }

    }

    public void SetJointActive(bool _bool)
    {
        if (_bool)
        {
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }
        else
        {
            joint.connectedBody = null;
            joint.xMotion = ConfigurableJointMotion.Free;
            joint.yMotion = ConfigurableJointMotion.Free;
            joint.zMotion = ConfigurableJointMotion.Free;
            joint.angularXMotion = ConfigurableJointMotion.Free;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Free;
        }
    }

    public void OpenHand(bool _bool)
    {
        if (_bool)
        {
            openHand.SetActive(true);
            closedhand.SetActive(false);
        }
        else
        {
            openHand.SetActive(false);
            closedhand.SetActive(true);
        }
    }

    public enum State
    {
        Smash,
        Reaching,
        Grabbing
    }
}
