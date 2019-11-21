using UnityEngine;

public class HumanScript : PhysicalObjectScript
{
    public override void SetUp()
    {
        base.SetUp();
        giantConstruction = GameManager.gameManager.GetGiantConstruction;
        spawningManager = GameManager.gameManager.GetHumanSpawningManager;
        attackFrequenceSystem = new FrequenceSystem(1 / timeBetweenTwoAttack);
        attackFrequenceSystem.SetUp(StartAttack);
    }

    public override void DestroyPhysicalObject()
    {
        spawningManager.DecreamentNumberOfHumans();
        base.DestroyPhysicalObject();
    }

    GiantConstructionScript giantConstruction;
    HumanSpawningManager spawningManager;
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
    bool isWalking = false;
    public void StopAct()
    {
        canAct = false;
    }
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
    [SerializeField] float timeBetweenTwoAttack = 1;
    [SerializeField] int damageAmount = 5;
    [SerializeField] HumanProjectileScript projectilePrefab = default;
    [SerializeField] float rangeAttackDistance = 4;
    [SerializeField] float rangeShootingOffset = 0.1f;
    [SerializeField] float rangeShootingForce = 5;
    bool isInConstructionZone = false;

    FrequenceSystem attackFrequenceSystem;

    public void UpdateAttackSystem()
    {
        if (!preparingAttack)
            attackFrequenceSystem.UpdateFrequence();
    }

    bool preparingAttack;
    public void StartAttack()
    {
        preparingAttack = true;

        if (humanAnimator != null)
            humanAnimator.SetTrigger("attack");
        else
            LaunchTrueAttack();
    }

    public void LaunchTrueAttack()
    {
        preparingAttack = false;

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
                newProjectile.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;
                newProjectile.LaunchProjectile(shootDirection * rangeShootingForce, damageAmount);
                break;
        }
    }

    [Header("Rendering")]
    [SerializeField] Transform rendererParent = default;
    [SerializeField] Animator humanAnimator = default;

    public void LookTowardConstruction()
    {
        Vector3 lookDirection = giantConstruction.transform.position - transform.position;
        lookDirection.y = 0;
        lookDirection.Normalize();
        float rotY = Mathf.Atan2(lookDirection.z, lookDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 90 - rotY, 0);
    }

    public override void Update()
    {
        base.Update();

        if (pendingDestroy)
            return;

        if (!canAct)
            return;

        LookTowardConstruction();

        if (NeedToMoveTowardTarget)
        {
            UpdateMove();

            if (!isWalking)
            {
                isWalking = true;
                if (humanAnimator != null)
                    humanAnimator.SetBool("walking", isWalking);
            }
        }
        else
        {
            if (isWalking)
            {
                isWalking = false;
                if (humanAnimator != null)
                    humanAnimator.SetBool("walking", isWalking);
            }

            UpdateAttackSystem();
        }
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