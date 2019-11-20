using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HumanProjectileScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody projectileBody = default;

    public void LaunchProjectile(Vector3 force)
    {
        if (projectileBody == null)
            projectileBody = GetComponent<Rigidbody>();

        projectileBody.AddForce(force, ForceMode.VelocityChange);
    }


}
