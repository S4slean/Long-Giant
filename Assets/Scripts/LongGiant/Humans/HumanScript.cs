using UnityEngine;

public class HumanScript : PhysicalObjectScript
{
    private void OnEnable()
    {
        GameManager.gameManager.OnGameWin += Flee;    }

    private void OnDisable()
    {
        GameManager.gameManager.OnGameWin -= Flee;
    }

    public override void SetUp()
    {
        GameManager gameManager = GameManager.gameManager;

        base.SetUp();
        giantConstruction = gameManager.GetGiantConstruction;
        spawningManager = gameManager.GetHumanSpawningManager;
        attackFrequenceSystem = new FrequenceSystem(1 / timeBetweenTwoAttack);
        attackFrequenceSystem.SetUp(StartAttack);

        if (gameManager.gameFinished)
            Flee();
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
    bool fleeing = false;
    public void StopAct()
    {
        canAct = false;
        objectBody.freezeRotation = false;
    }

    public void Flee()
    {
        fleeing = true;
    }

    public void UpdateMove()
    {
        Vector3 moveDirection = giantConstruction.transform.position - transform.position;
        moveDirection.y = 0;
        Vector3 move = moveDirection.normalized * moveSpeed * Time.deltaTime * (fleeing ? -1.5f : 1);
        move.y = objectBody.velocity.y;
        objectBody.velocity = move;

        Debug.DrawRay(transform.position, moveDirection, Color.red);
    }

    public bool NeedToMoveTowardTarget
    {
        get
        {
            if (fleeing)
                return true;

            switch (attackType)
            {
                case HumanAttackType.Melee:
                    return GetDistanceWithGiant > meleeAttackDistance;
                //return !isInConstructionZone;
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
    [SerializeField] float meleeAttackDistance = 2.75f;
    [SerializeField] HumanProjectileScript projectilePrefab = default;
    [SerializeField] float rangeAttackDistance = 4;
    [SerializeField] float rangeShootingOffset = 0.1f;
    [SerializeField] float rangeShootingForce = 5;
    //bool isInConstructionZone = false;

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
        if (!canAct)
            return;

        Vector3 lookDirection = giantConstruction.transform.position - transform.position;
        lookDirection.y = 0;
        lookDirection.Normalize();
        float rotY = Mathf.Atan2(lookDirection.z, lookDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 90 - rotY * (fleeing ? -1 : 1), 0);
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
}

public enum HumanAttackType
{
    Melee, Range
}