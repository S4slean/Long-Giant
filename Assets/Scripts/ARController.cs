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

    public GameObject m_arCoreDevice;
    public GameObject m_worldOriginPrefab;
    public GameObject m_shadowPlane;
    public GameObject m_terrainPlane;
    public GameObject m_worldBoundPrefab;

    private Anchor m_anchorRoot;
    private GameObject m_worldRootBeacon;
    private float m_radius = 0.0f;
    private bool m_isInitied = false;

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
        m_shadowPlane.transform.SetParent(m_anchorRoot.transform);
        m_shadowPlane.transform.position = position;
        float worldSize = m_radius * 1.1f * 2.0f;
        m_terrainPlane.transform.localScale = new Vector3(worldSize, 0.04f, worldSize);
        m_shadowPlane.SetActive(true);

        GameObject go;
        Vector3 size = new Vector3(worldSize * 2.0f, 10.0f, worldSize * 2.0f);
        Vector3 center = new Vector3(position.x, position.y + (size.y / 2.0f) - 0.1f, position.z);

        /// ====================== ///
        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, -size.y / 2.0f, 0), Quaternion.identity);
        go.transform.localScale = new Vector3(size.x, 0.2f, size.z);
        go.transform.SetParent(m_anchorRoot.transform);
        go.layer = 0;
        go.name = "Bound DOWN";

        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, size.y / 2.0f, 0), Quaternion.identity);
        go.transform.localScale = new Vector3(size.x, 0.2f, size.z);
        go.transform.SetParent(m_anchorRoot.transform);
        go.layer = 8;
        go.name = "Bound UP";

        /// ====================== ///
        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, 0, -size.z / 2.0f), Quaternion.identity);
        go.transform.localScale = new Vector3(size.x, size.y, 0.2f);
        go.transform.SetParent(m_anchorRoot.transform);
        go.layer = 8;
        go.name = "Bound LEFT";

        go = Instantiate(m_worldBoundPrefab, center + new Vector3(0, 0, size.x / 2.0f), Quaternion.identity);
        go.transform.localScale = new Vector3(size.x, size.y, 0.2f);
        go.transform.SetParent(m_anchorRoot.transform);
        go.layer = 8;
        go.name = "Bound RIGHT";

        /// ====================== ///
        go = Instantiate(m_worldBoundPrefab, center + new Vector3(-size.x / 2.0f, 0, 0), Quaternion.identity);
        go.transform.localScale = new Vector3(0.2f, size.y, size.z);
        go.transform.SetParent(m_anchorRoot.transform);
        go.name = "Bound FOREWARD";
        go.layer = 8;

        go = Instantiate(m_worldBoundPrefab, center + new Vector3(size.x / 2.0f, 0, 0), Quaternion.identity);
        go.transform.localScale = new Vector3(0.2f, size.y, size.z);
        go.transform.SetParent(m_anchorRoot.transform);
        go.name = "Bound BACKWARD";
        go.layer = 8;
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
                return;
            }

            //Pose pose = new Pose(hit.point, Quaternion.LookRotation(hit.normal));
            Pose pose = new Pose(hit.point, Quaternion.identity);

            if (m_anchorRoot == null) // Is first touch
            {
                m_anchorRoot = Session.CreateAnchor(pose);
                m_anchorRoot.gameObject.name = "Root Anchor";
                //m_anchorRoot.transform.rotation = Quaternion.identity;
                m_anchorRoot.transform.SetParent(m_arCoreDevice.transform);

                m_worldRootBeacon = Instantiate(m_worldOriginPrefab, new Vector3(pose.position.x, pose.position.y, pose.position.z), Quaternion.Euler(-90.0f, 0, 0));
                m_worldRootBeacon.transform.localScale = new Vector3(1f, 1f, 1f);
                m_worldRootBeacon.transform.SetParent(m_anchorRoot.transform);
            }
            else // Is second touch
            {
                var tmpPos = new Vector3(pose.position.x, m_anchorRoot.transform.position.y, pose.position.z);
                m_radius = Vector3.Distance(m_anchorRoot.transform.position, tmpPos);
                /*if (m_radius < 1f) // TODO: uncomment this when tests are done
                {
                    return;
                }*/
                m_isInitied = true;
                SetShadowPlanePosition(m_anchorRoot.transform.position);

                StartCoroutine(RemoveWorldRootBeacon());
                StartCoroutine(SonarRings());
            }
        }
    }

    IEnumerator SonarRings()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (var item in Object.FindObjectsOfType<SimpleSonarShader_Object>())
            {
                yield return new WaitForSeconds(0.1f);
                item.StartSonarRing(m_anchorRoot.transform.position, 4.0f);
            }
            yield return new WaitForSeconds(i * 0.3f);
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