using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public PhysicalObjectScript m_prefabBall;
    public float force = 15;

    private void Start()
    {
        
    }

    private void Update()
    {
        UserInputs();
    }

    private void UserInputs()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PhysicalObjectScript ball = Instantiate(m_prefabBall, transform.position, Quaternion.identity);
            ball.transform.rotation = Quaternion.LookRotation(transform.forward);
            ball.GetComponent<Rigidbody>().AddForce(ball.transform.forward * force, ForceMode.VelocityChange);
        }
    }
}
