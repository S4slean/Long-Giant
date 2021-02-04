using UnityEngine;
using System.Collections.Generic;

public class HumanScript : PhysicalObjectScript
{
    float maxHumans = 0;
    float delayAfterSpawn = 0.75f;

    GiantConstructionScript giantConstruction;
    HumanSpawningManager spawningManager;


    [Header("Movements")]
    [SerializeField] float moveSpeed = 10;
    bool canAct = true;
    bool isWalking = false;
    bool fleeing = false;


    [Header("Damaging")]
    [SerializeField] HumanAttackType attackType = HumanAttackType.Melee;
    [SerializeField] float timeBetweenTwoAttack = 1;
    [SerializeField] int damageAmount = 5;
    [SerializeField] float minDistanceWithConstruction = 4;
    [SerializeField] float meleeAttackDistance = 5;
    [SerializeField] HumanProjectileScript projectilePrefab = default;
    [SerializeField] float rangeAttackDistance = 8;
    [SerializeField] float rangeShootingOffset = 0.1f;
    [SerializeField] float rangeShootingForce = 5;

    FrequenceSystem attackFrequenceSystem;
    bool preparingAttack;
    bool playAttackSound = false;


    [Header("Rendering")]
    [SerializeField] Rigidbody humanWeapon = default;
    [SerializeField] List<Collider> allWeaponColliders = default;

    [SerializeField] Transform rendererParent = default;
    [SerializeField] Animator humanAnimator = default;
    [SerializeField] Animator humanBubbleAnimator = default;

    [SerializeField] AudioSource preparingAttackSoundSource = default;
    [SerializeField] AudioSource attackSoundSource = default;
    [SerializeField] AudioSource fleeSoundSource = default;


    #region Engine Callbacks

    private void OnEnable()
    {
        //Make the human flee when game is over
        GameManager.gameManager.OnGameWin += Flee;
    }

    private void OnDisable()
    {
        //Clear subscribe on disabled
        GameManager.gameManager.OnGameWin -= Flee;
    }

    /// <summary>
    /// Updates the Human global behaviour
    /// </summary>
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
            //Update movement if Human must move to target
            UpdateMove();

            if (!isWalking && delayAfterSpawn == 0)
            {
                isWalking = true;
                if (humanAnimator != null)
                    humanAnimator.SetBool("walking", isWalking);
            }
        }
        else
        {
            //Update Attack System if Human musn't move to target
            UpdateAttackSystem();

            objectBody.velocity = new Vector3(0, objectBody.velocity.y, 0);

            if (isWalking)
            {
                isWalking = false;
                if (humanAnimator != null)
                    humanAnimator.SetBool("walking", isWalking);
            }
        }
    }

    #endregion



    /// <summary>
    /// Override SetUp to set up attacks systems and animations
    /// </summary>
    public override void SetUp()
    {
        GameManager gameManager = GameManager.gameManager;

        base.SetUp();

        //Get references
        giantConstruction = gameManager.GetGiantConstruction;
        spawningManager = gameManager.GetHumanSpawningManager;
        maxHumans = spawningManager.maximumNumberOfHumans;

        //Set up attack frequence system
        attackFrequenceSystem = new FrequenceSystem(1 / timeBetweenTwoAttack);
        attackFrequenceSystem.SetUp(StartAttack);
        attackFrequenceSystem.SetFrequenceProgression(0.2f);

        //Disable weapon's physic
        if (humanWeapon != null)
            humanWeapon.isKinematic = true;
        foreach (Collider coll in allWeaponColliders)
            coll.enabled = false;

        //If Human got spawned once game was finished (because player destroyed a Construction, for example), make it flee instantly
        if (gameManager.gameFinished)
            Flee();

        //Play the angry animation on spawn
        if(humanBubbleAnimator != null && !gameManager.gameFinished)
        {
            humanBubbleAnimator.SetInteger("angryCounter", Random.Range(0, 2));
            humanBubbleAnimator.SetTrigger("spawned");
        }
    }

    /// <summary>
    /// Override DestroyPhysicalObject to decreament the current number of humans on the Spawning Manager
    /// </summary>
    public override void DestroyPhysicalObject()
    {
        spawningManager.DecreamentNumberOfHumans();
        base.DestroyPhysicalObject();
    }


    /// <summary>
    /// Prevent the Human from acting, likely because it got grabbed.
    /// </summary>
    public void StopAct()
    {
        canAct = false;
        objectBody.freezeRotation = false;

        isWalking = false;
        if (humanAnimator != null)
            humanAnimator.SetBool("walking", isWalking);

        if (humanBubbleAnimator != null)
            humanBubbleAnimator.SetTrigger("grabbed");
    }


    #region Movements

    /// <summary>
    /// Gets the flat distance between the human and the giant construction's center
    /// </summary>
    public float GetDistanceWithGiantConstruction
    {
        get
        {
            Vector3 difference = giantConstruction.transform.position - transform.position;
            difference.y = 0;
            return difference.magnitude;
        }
    }

    /// <summary>
    /// Processed if the human should be moving toward its target
    /// </summary>
    public bool NeedToMoveTowardTarget
    {
        get
        {
            //If fleeing, Human will move away from the giant construction
            if (fleeing)
                return true;

            //If too close from the giant construction, Human will move away from the it
            if (GetDistanceWithGiantConstruction < minDistanceWithConstruction)
                return true;

            //Else, return true if distance with target is higher than the attack distance (depending in enemy's type)
            switch (attackType)
            {
                case HumanAttackType.Melee:
                    return GetDistanceWithGiantConstruction > meleeAttackDistance;
                case HumanAttackType.Range:
                    return GetDistanceWithGiantConstruction > rangeAttackDistance;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Update Human movement toward its target
    /// </summary>
    public void UpdateMove()
    {
        //Avoid movement for a few moment after spawning
        if (delayAfterSpawn > 0)
        {
            delayAfterSpawn -= Time.deltaTime;

            if (delayAfterSpawn < 0)
                delayAfterSpawn = 0;

            return;
        }

        //Get the base movement direction, and multiply it by a certain movement speed multiplier depeding on the current Human's state (fleeing = negative speed)
        Vector3 moveDirection = giantConstruction.transform.position - transform.position;
        moveDirection.y = 0;

        float movementSpeedMultiplier = (fleeing ? -1.5f : GetDistanceWithGiantConstruction < minDistanceWithConstruction ? -0.75f : 1);

        Vector3 move = moveDirection.normalized * moveSpeed * Time.deltaTime * movementSpeedMultiplier;
        move.y = objectBody.velocity.y;
        objectBody.velocity = move;
    }

    /// <summary>
    /// Make the Human flee, dropping its weapon and playing some feedbacks
    /// </summary>
    public void Flee()
    {
        fleeing = true;

        //Throw the human's weapon
        if (humanWeapon != null)
        {
            humanWeapon.isKinematic = false;
            humanWeapon.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;

            foreach (Collider coll in allWeaponColliders)
                coll.enabled = true;

            Vector3 randomThrowVelocity = Random.onUnitSphere;
            randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);
            randomThrowVelocity = Vector3.Slerp(randomThrowVelocity, Vector3.up, 0.5f);

            humanWeapon.AddForce(randomThrowVelocity * Random.Range(8f, 10f));
            humanWeapon.AddTorque(Random.onUnitSphere * Random.Range(180f, 270f));
        }

        //Play fleeing away animation
        if (humanBubbleAnimator != null)
            humanBubbleAnimator.SetTrigger("fleeing");

        //Play fleeing away sound
        if (fleeSoundSource != null)
        {
            fleeSoundSource.pitch = Random.Range(0.9f, 1.2f);
            fleeSoundSource.Play();
        }
    }

    #endregion


    #region Attack

    /// <summary>
    /// Updates the attack frequence
    /// </summary>
    public void UpdateAttackSystem()
    {
        if (!preparingAttack)
            attackFrequenceSystem.UpdateFrequence();
    }

    /// <summary>
    /// Start the Attack execution, by playing the attack animation, which should trigger the real attack with an animation event
    /// </summary>
    public void StartAttack()
    {
        preparingAttack = true;

        //Starts attack on animator if exists - If not, just launch the attack directly
        if (humanAnimator != null)
            humanAnimator.SetTrigger("attack");
        else
            LaunchTrueAttack();

        if (humanBubbleAnimator != null)
            humanBubbleAnimator.SetTrigger("attack");

        //Play attack preparation sound source - Add some probabilities to play it in order ot prevent all humans from playing this sound if there are a lot of them
        if (preparingAttackSoundSource != null)
        {
            float numberOfHumans = GameManager.gameManager.GetHumanSpawningManager.currentNumberOfHumans;

            //Probability to play the sound will always be 1 if there is only one human
            float proba = numberOfHumans > 0 ? Mathf.Clamp(1 - ((numberOfHumans - 1) / (maxHumans / 2)), 0.2f, 1) : 1;
            float random = Random.Range(0f, 1f);

            if (random < proba)
            {
                playAttackSound = true;
                preparingAttackSoundSource.pitch = Random.Range(0.9f, 1.2f);
                preparingAttackSoundSource.Play();
            }
            else
                playAttackSound = false;
        }
    }

    /// <summary>
    /// Launches the actual attack, playing the attack effects
    /// </summary>
    public void LaunchTrueAttack()
    {
        //Play sound
        if (attackSoundSource != null && playAttackSound)
        {
            attackSoundSource.pitch = Random.Range(0.9f, 1.2f);
            attackSoundSource.Play();
        }

        preparingAttack = false;

        switch (attackType)
        {
            //Melee Humans just affect damages instantly 
            case HumanAttackType.Melee:
                giantConstruction.ReceiveDamages(damageAmount);
                break;

            //Range Humans launches a projectile toward the construction - Process a direction that will throw projectile in a curved trajectory
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

    #endregion

        
    /// <summary>
    /// Make the human look at the Giant Construction
    /// </summary>
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


    /// <summary>
    /// Retrieves all Human's Weapon's sub-colliders
    /// </summary>
    [ContextMenu("GetAllWeaponColliders")]
    public void GetAllWeaponColliders()
    {
        allWeaponColliders = new List<Collider>();

        if (humanWeapon == null)
            return;

        BoxCollider[] boxes = humanWeapon.GetComponentsInChildren<BoxCollider>();
        SphereCollider[] spheres = humanWeapon.GetComponentsInChildren<SphereCollider>();
        CapsuleCollider[] caspules = humanWeapon.GetComponentsInChildren<CapsuleCollider>();
        MeshCollider[] meshes = humanWeapon.GetComponentsInChildren<MeshCollider>();

        foreach (BoxCollider coll in boxes)
            allWeaponColliders.Add(coll);

        foreach (SphereCollider coll in spheres)
            allWeaponColliders.Add(coll);

        foreach (CapsuleCollider coll in caspules)
            allWeaponColliders.Add(coll);

        foreach (MeshCollider coll in meshes)
            allWeaponColliders.Add(coll);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}

/// <summary>
/// Type of Human enemy
/// </summary>
public enum HumanAttackType
{
    Melee, Range
}