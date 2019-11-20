using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanScript : PhysicalObjectScript
{
    public override void SetUp()
    {
        base.SetUp();
        giantConstruction = GameManager.gameManager.GetGiantConstruction;
        attackFrequenceSystem = new FrequenceSystem(attackFrequence);
        attackFrequenceSystem.SetUp(Attack);
    }

    GiantConstructionScript giantConstruction;
    public float GetDistanceWithGiant 
    { 
        get 
        { 
            Vector3 difference = giantConstruction.transform.position - transform.position; 
            difference.y = 0; 
            return difference.magnitude;
        } 
    }

    [Header("Movements")]
    [SerializeField] float moveSpeed = 10;
    bool canAct = true;
    public void UpdateMove()
    {
        Vector3 moveDirection = giantConstruction.transform.position - transform.position;
        moveDirection.y = 0;
        objectBody.velocity = moveDirection.normalized * moveSpeed * Time.deltaTime;

        Debug.DrawRay(transform.position, moveDirection, Color.red);
    }

    public bool NeedToMoveTowardTarget
    {
        get
        {
            switch (attackType)
            {
                case HumanAttackType.Melee:
                    return !isInConstructionZone;
                case HumanAttackType.Range:
                    return GetDistanceWithGiant > rangeAttackDistance;
                default:
                    return false;
            }
        }
    }

    [Header("Damaging")]
    [SerializeField] HumanAttackType attackType = HumanAttackType.Melee;
    [SerializeField] float attackFrequence = 1;
    [SerializeField] int damageAmount = 5;
    [SerializeField] HumanProjectileScript projectilePrefab = default;
    [SerializeField] float rangeAttackDistance = 4;
    [SerializeField] float rangeShootingOffset = 0.1f;
    [SerializeField] float rangeShootingForce = 200;
    bool isInConstructionZone = false;

    FrequenceSystem attackFrequenceSystem;

    public void UpdateAttackSystem()
    {
        attackFrequenceSystem.UpdateFrequence();
    }

    public void Attack()
    {
        switch (attackType)
        {
            case HumanAttackType.Melee:
                giantConstruction.ReceiveDamages(damageAmount);
                break;

            case HumanAttackType.Range:
                Vector3 shootDirection = giantConstruction.transform.position - transform.position;
                shootDirection.y = 0;
                shootDirection.Normalize();
                Vector3 sideVector = new Vector3(-shootDirection.z, 0, shootDirection.x);
                shootDirection = Quaternion.AngleAxis(45, sideVector) * shootDirection;
                HumanProjectileScript newProjectile = Instantiate(projectilePrefab, transform.position + shootDirection * rangeShootingOffset, Quaternion.identity);
                newProjectile.LaunchProjectile(shootDirection * rangeShootingForce, damageAmount);
                break;
        }
    }

    public void Update()
    {
        if (!canAct)
            return;

        if (NeedToMoveTowardTarget)
            UpdateMove();
        else
            UpdateAttackSystem();
    }

    private void OnTriggerEnter(Collider other)
    {
        GiantConstructionScript giantConstruction = other.GetComponent<GiantConstructionScript>();
        if (giantConstruction != null)
            isInConstructionZone = true;
    }
}

public enum HumanAttackType
{
    Melee, Range
}