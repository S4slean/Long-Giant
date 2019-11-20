using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HumanProjectileScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Rigidbody projectileBody = default;
    [SerializeField] int damagesAmount = 5;

    public void LaunchProjectile(Vector3 force, int damages)
    {
        if (projectileBody == null)
            projectileBody = GetComponent<Rigidbody>();

        projectileBody.AddForce(force, ForceMode.VelocityChange);

        damagesAmount = damages;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GiantConstructionSolidColliderScript giantConstructionSolidCollider = collision.collider.GetComponent<GiantConstructionSolidColliderScript>();
        if(giantConstructionSolidCollider != null)
        {
            giantConstructionSolidCollider.GetGiantConstruction.ReceiveDamages(damagesAmount);
            Destroy(gameObject);
        }
    }
}
