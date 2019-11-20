using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using System;

public class Main : MonoBehaviour
{
    public GameObject m_shadowPlane;
    public GameObject m_drone;


    private List<DetectedPlane> m_detectedPlanes = new List<DetectedPlane>();
    private bool m_initDone = false;

    private void Awake()
    {
        m_initDone = false;
        m_shadowPlane.SetActive(false);
    }

    void Update()
    {
        WatchForShadowlaneInit();
    }

    private void WatchForShadowlaneInit()
    {
        if (m_initDone)
            return;

        Session.GetTrackables<DetectedPlane>(m_detectedPlanes, TrackableQueryFilter.All);

        if (m_detectedPlanes.Count > 0)
        {
            foreach (DetectedPlane plane in m_detectedPlanes)
            {
                if (plane.PlaneType == DetectedPlaneType.HorizontalUpwardFacing)
                {


                    SetShadowPlanePosition(m_detectedPlanes[0].CenterPose.position);
                    DroneInit(m_detectedPlanes[0].CenterPose.position);

                    m_initDone = true;
                }
            }
        }
    }

    private void SetShadowPlanePosition(Vector3 position)
    {
        m_shadowPlane.transform.position = position;
        m_shadowPlane.SetActive(true);
    }
    private void DroneInit(Vector3 floorPosition)
    {
        Vector3 cameraForward = GetFlatHorizontalDirection(Camera.main.transform.forward);
        Vector3 spawnPosition = Camera.main.transform.position + cameraForward * 1.5f;
        spawnPosition = new Vector3(spawnPosition.x, floorPosition.y - 1f, spawnPosition.z);
        m_drone.transform.position = spawnPosition;

        StartCoroutine(DroneAnimation(spawnPosition + Vector3.up * 2f));
    }

    private IEnumerator DroneAnimation(Vector3 targetPosition)
    {
        float timer = 0f;
        Vector3 startPosition = m_drone.transform.position;

        while (timer < 1f)
        {
            timer += Time.deltaTime * 0.25f;
            m_drone.transform.position = Vector3.Lerp(startPosition, targetPosition, timer);
            yield return null;
        }
    }

    private Vector3 GetFlatHorizontalDirection(Vector3 dir)
    {
        return new Vector3(dir.x, 0f, dir.z);
    }
}
