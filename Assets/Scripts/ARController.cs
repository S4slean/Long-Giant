using System;
using UnityEngine;
using UnityEngine.EventSystems;
using GoogleARCore;
using GoogleARCore.Examples.Common;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

struct TrackingState
{
    public bool previous;
    public bool current;
}


public class ARController : MonoBehaviour
{
    public Camera playerCamera;

    public GameObject m_arCoreDevice;
    public GameObject m_worldOriginPrefab;
    public GameObject m_shadowPlane;
    public GameObject m_worldGroundPrefab;
    public GameObject m_worldBoundPrefab;
    public float m_minPlaygroundRadius = 7.5f;
    public LayerMask layerMask;

    private Anchor m_anchorRoot;
    private GameObject m_worldRootBeacon;
    private float m_radius = 0.0f;
    private bool m_isInitied = false;
    private bool m_IsQuitting = false;

    private GameObject m_planeDiscovery;
    private GameObject m_planeGenerator;
    private GameObject m_pointCloud;

    private TrackingState m_trackingState;

    public void Awake()
    {
        // Enable ARCore to target 60fps camera capture frame rate on supported devices.
        // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
        Application.targetFrameRate = 60;
        if (m_shadowPlane != null)
        {
            m_shadowPlane.SetActive(false);
        }

        m_planeDiscovery = UnityEngine.Object.FindObjectOfType<PlaneDiscoveryGuide>().gameObject;
        m_planeGenerator = UnityEngine.Object.FindObjectOfType<DetectedPlaneGenerator>().gameObject;
        m_pointCloud = UnityEngine.Object.FindObjectOfType<PointcloudVisualizer>().gameObject;
    }

    private void SetShadowPlanePosition(Vector3 position)
    {
        Debug.Log("Radius is: " + m_radius);
        if (m_shadowPlane != null)
        {
            m_shadowPlane.transform.SetParent(m_anchorRoot.transform);
            m_shadowPlane.transform.position = position;
            m_shadowPlane.SetActive(true);
        }

        float worldSize = m_radius * 1.1f * 2.0f;

        GameObject go;
        Vector3 size = new Vector3(worldSize * 2.0f, 10.0f, worldSize * 2.0f);
        Vector3 center = new Vector3(position.x, position.y + (size.y / 2.0f) - 0.1f, position.z);

        /// ====================== ///
        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, -size.y / 2.0f, 0), Quaternion.identity, m_anchorRoot.transform);
        go.transform.localScale = new Vector3(size.x, 0.2f, size.z);
        go.layer = 0;
        go.name = "Bound DOWN";

        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, size.y / 2.0f, 0), Quaternion.identity, m_anchorRoot.transform);
        go.transform.localScale = new Vector3(size.x, 0.2f, size.z);
        go.name = "Bound UP";

        /// ====================== ///
        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, 0, -size.z / 2.0f), Quaternion.identity, m_anchorRoot.transform);
        go.transform.localScale = new Vector3(size.x, size.y, 0.2f);
        go.name = "Bound LEFT";

        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, 0, size.x / 2.0f), Quaternion.identity, m_anchorRoot.transform);
        go.transform.localScale = new Vector3(size.x, size.y, 0.2f);
        go.name = "Bound RIGHT";

        /// ====================== ///
        go = Instantiate(m_worldBoundPrefab, center + new Vector3(-size.x / 2.0f, 0, 0), Quaternion.identity, m_anchorRoot.transform);
        go.transform.localScale = new Vector3(0.2f, size.y, size.z);
        go.name = "Bound FOREWARD";

        go = Instantiate(m_worldBoundPrefab, center + new Vector3(size.x / 2.0f, 0, 0), Quaternion.identity, m_anchorRoot.transform);
        go.transform.localScale = new Vector3(0.2f, size.y, size.z);
        go.name = "Bound BACKWARD";
    }

    private void OnTrackingRecovered()
    {
        Debug.Log("OnTrackingRecovered() Desactivate planes");
        m_planeDiscovery.SetActive(false);
        m_planeGenerator.SetActive(false);
        m_pointCloud.SetActive(false);
    }

    private void OnTrackingLost()
    {
        Debug.Log("OnTrackingLost() Activate planes");
        m_planeDiscovery.SetActive(true);
        m_planeGenerator.SetActive(true);
        m_pointCloud.SetActive(true);
    }

    public void Update()
    {
        _UpdateApplicationLifecycle();

        if (m_isInitied) // The two points were set
        {
            return;
        }

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

        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask.value))
        {
            Pose pose = new Pose(hit.point, Quaternion.identity);

            if (m_anchorRoot == null) // Is first touch
            {
                m_anchorRoot = Session.CreateAnchor(pose);
                m_anchorRoot.gameObject.name = "Root Anchor";
                m_anchorRoot.transform.SetParent(m_arCoreDevice.transform);
                GameManager.gameManager.SetAllGameObjectsParent(m_anchorRoot.transform);

                m_worldRootBeacon = Instantiate(m_worldOriginPrefab, new Vector3(pose.position.x, pose.position.y, pose.position.z), Quaternion.Euler(-90f, 0, 0));
                m_worldRootBeacon.transform.localScale = new Vector3(1f, 1f, 1f);
                m_worldRootBeacon.transform.SetParent(m_anchorRoot.transform);
            }
            else // Is second touch
            {
                var tmpPos = new Vector3(pose.position.x, m_anchorRoot.transform.position.y, pose.position.z);
                m_radius = Vector3.Distance(m_anchorRoot.transform.position, tmpPos);
                if (m_radius < m_minPlaygroundRadius)
                {
                    _ShowAndroidToastMessage("Playground radius should be >= " + String.Format("{0:0.00}", m_minPlaygroundRadius / 10f) + " meters ; currently " + String.Format("{0:0.00}", m_radius / 10f) + " meters");
                    return;
                }
                m_isInitied = true;
                SetShadowPlanePosition(m_anchorRoot.transform.position);

                Destroy(m_worldRootBeacon);

                GameManager.gameManager.GetWorldGenerationManager.GenerateWorld(m_anchorRoot.transform.position, m_radius);

                OnTrackingRecovered();
            }
        }
    }

    // Create a post at a position
    // Pose pose = new Pose(position, rotation);
    // Anchor anchor = plane.CreateAnchor(pose);
    // gameObject.transform.SetParent(anchor.transform);

    private void _UpdateApplicationLifecycle()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            if (m_isInitied)
            {
                m_trackingState.previous = m_trackingState.current;
                m_trackingState.current = false;
                if (m_trackingState.current != m_trackingState.previous)
                {
                    OnTrackingLost();
                }
            }
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            if (m_isInitied)
            {
                m_trackingState.previous = m_trackingState.current;
                m_trackingState.current = true;
                if (m_trackingState.current != m_trackingState.previous)
                {
                    OnTrackingRecovered();
                }
            }
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
            _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
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