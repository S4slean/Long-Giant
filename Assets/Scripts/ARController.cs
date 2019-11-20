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

public class ARController : MonoBehaviour
{
    public Camera playerCamera;

    private bool m_IsQuitting = false;

    public GameObject m_worldOriginPrefab;
    public GameObject m_shadowPlane;

    private Anchor m_anchorRoot;
    private float m_radius = 0.0f;

    public void Awake()
    {
        // Enable ARCore to target 60fps camera capture frame rate on supported devices.
        // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
        Application.targetFrameRate = 60;
        m_shadowPlane.SetActive(false);
    }

    private void SetShadowPlanePosition(Vector3 position)
    {
        Debug.Log("Radius is: " + m_radius);
        //m_shadowPlane.transform.SetParent(m_anchorRoot.transform);
        m_shadowPlane.transform.position = position;
        //m_shadowPlane.transform.Find("OcclusionPlane/Plane").transform.localScale = new Vector3(m_radius, 1, m_radius);
        m_shadowPlane.SetActive(true);
    }

    public void Update()
    {
        _UpdateApplicationLifecycle();

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;

        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(playerCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                if (m_radius != 0.0f) // The two points were set
                {
                    // TODO: Temporary:
                    playerCamera.GetComponent<DepthPostprocessing>().waveActive = !playerCamera.GetComponent<DepthPostprocessing>().waveActive;
                    return;
                }

                if (m_anchorRoot == null) // Is first touch
                {
                    m_anchorRoot = hit.Trackable.CreateAnchor(hit.Pose);
                    m_anchorRoot.gameObject.name = "Root Anchor";

                    GameObject beacon = Instantiate(m_worldOriginPrefab, new Vector3(hit.Pose.position.x, hit.Pose.position.y + 0.1f, hit.Pose.position.z), Quaternion.Euler(90, 0, 0));
                    beacon.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                    beacon.transform.SetParent(m_anchorRoot.transform);
                }
                else // Is second touch
                {
                    var tmpPos = new Vector3(hit.Pose.position.x, m_anchorRoot.transform.position.y, hit.Pose.position.z);
                    m_radius = Vector3.Distance(m_anchorRoot.transform.position, tmpPos);
                    SetShadowPlanePosition(m_anchorRoot.transform.position);
                }
            }
        }
    }

    public Anchor getRootAnchor()
    {
        return m_anchorRoot;
    }

    // Create a post at a position
    // Pose pose = new Pose(position, rotation);
    // Anchor anchor = plane.CreateAnchor(pose);
    // gameObject.transform.SetParent(anchor.transform);

    private void _UpdateApplicationLifecycle()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (m_IsQuitting)
        {
            return;
        }

        // Quit if ARCore was unable to connect and give Unity some time for the toast to
        // appear.
        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            _ShowAndroidToastMessage("Camera permission is needed to run this application.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            _ShowAndroidToastMessage(
                "ARCore encountered a problem connecting.  Please start the app again.");
            m_IsQuitting = true;
            Invoke("_DoQuit", 0.5f);
        }
    }

    private void _DoQuit()
    {
        Application.Quit();
    }

    private void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}