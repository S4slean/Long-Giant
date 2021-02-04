using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple projectile script for humans projectiles
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class HumanProjectileScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody projectileBody = default;
    [SerializeField] int damagesAmount = 5;

    /// <summary>
    /// Callback used to check if projectil hit the GiantConstruction - If it did, inflict damages and destroy projectile
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        GiantConstructionSolidColliderScript giantConstructionSolidCollider = collision.collider.GetComponent<GiantConstructionSolidColliderScript>();
        if (giantConstructionSolidCollider != null)
        {
            giantConstructionSolidCollider.GetGiantConstruction.ReceiveDamages(damagesAmount);
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Throws the projectile with the inputed force, and sets up the damages value.
    /// </summary>
    /// <param name="force"></param>
    /// <param name="damages"></param>
    public void LaunchProjectile(Vector3 force, int damages)
    {
        if (projectileBody == null)
            projectileBody = GetComponent<Rigidbody>();

        projectileBody.AddForce(force, ForceMode.VelocityChange);

        damagesAmount = damages;
    }
}
