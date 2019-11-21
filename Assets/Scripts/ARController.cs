using System;
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
    public Action<Vector3, float> OnPlaygroundCreated;

    public Camera playerCamera;

    private bool m_IsQuitting = false;

    public GameObject m_arCoreDevice;
    public GameObject m_worldOriginPrefab;
    public GameObject m_shadowPlane;
    public GameObject m_worldGroundPrefab;
    public GameObject m_worldBoundPrefab;
    public float m_minPlaygroundRadius = 7.5f;

    private GameObject m_planeDiscovery;
    private GameObject m_planeGenerator;
    private GameObject m_pointCloud;

    private Anchor m_anchorRoot;
    private GameObject m_worldRootBeacon;
    private float m_radius = 0.0f;
    private bool m_isInitied = false;

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
        /*GameObject terrainPlane = Instantiate(m_worldGroundPrefab, position, Quaternion.identity, m_anchorRoot.transform);
        terrainPlane.transform.localScale = new Vector3(worldSize, 0.04f, worldSize);
        terrainPlane.name = "WorldGround";*/

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

        if (Physics.Raycast(ray, out hit))
        {
            if (m_isInitied) // The two points were set
            {
                SonarRings();
                return;
            }

            Pose pose = new Pose(hit.point, Quaternion.identity);

            if (m_anchorRoot == null) // Is first touch
            {
                m_anchorRoot = Session.CreateAnchor(pose);
                m_anchorRoot.gameObject.name = "Root Anchor";
                m_anchorRoot.transform.SetParent(m_arCoreDevice.transform);

                m_worldRootBeacon = Instantiate(m_worldOriginPrefab, new Vector3(pose.position.x, pose.position.y, pose.position.z), Quaternion.Euler(-90.0f, 0, 0));
                m_worldRootBeacon.transform.localScale = new Vector3(1f, 1f, 1f);
                m_worldRootBeacon.transform.SetParent(m_anchorRoot.transform);
            }
            else // Is second touch
            {
                var tmpPos = new Vector3(pose.position.x, m_anchorRoot.transform.position.y, pose.position.z);
                m_radius = Vector3.Distance(m_anchorRoot.transform.position, tmpPos);
                if (m_radius < m_minPlaygroundRadius)
                {
                    _ShowAndroidToastMessage("Playground radius should be > 1= meter ; currently " + m_radius/10f + " meters");
                    return;
                }
                m_isInitied = true;
                SetShadowPlanePosition(m_anchorRoot.transform.position);

                //SonarRings();
                StartCoroutine(RemoveWorldRootBeacon());

                GameManager.gameManager.GetWorldGenerationManager.GenerateWorld(m_anchorRoot.transform.position, m_radius);

                OnTrackingRecovered();

                /*if (OnPlaygroundCreated != null)
                {
                    OnPlaygroundCreated(m_anchorRoot.transform.position, m_radius);
                }*/
            }
        }
    }

    void SonarRings()
    {
        foreach (var item in UnityEngine.Object.FindObjectsOfType<SimpleSonarShader_Object>())
        {
            StartCoroutine(SonarRing(item));
        }
    }

    IEnumerator SonarRing(SimpleSonarShader_Object item)
    {
        for (int i = 0; i < 12; i++)
        {
            item.StartSonarRing(m_anchorRoot.transform.position, 4.0f);
            yield return new WaitForSeconds(i * i * 1.7f);
            //yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RemoveWorldRootBeacon()
    {
        while (m_worldRootBeacon.transform.localScale.magnitude > 0.1f)
        {
            m_worldRootBeacon.transform.localScale *= 0.85f;
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(m_worldRootBeacon);
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
                OnTrackingLost();
            }
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        else
        {
            if (m_isInitied)
            {
                OnTrackingRecovered();
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