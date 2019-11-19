using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using System;

#if UNITY_EDITOR
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class BallLauncher : MonoBehaviour
{
    public GameObject m_prefabBall;

    private void Start()
    {
        
    }

    private void Update()
    {
        UserInputs();
    }

    private void UserInputs()
    {
        if(Input.touchCount> 0)
        {
            Touch touch = Input.touches[0];

            if(touch.phase == TouchPhase.Began)
            {
                GameObject ball = Instantiate(m_prefabBall, Camera.main.transform.position, Quaternion.identity) as GameObject;
                ball.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
                ball.GetComponent<Rigidbody>().AddForce(ball.transform.forward * 15f, ForceMode.Impulse);
            }
        }
    }
}
