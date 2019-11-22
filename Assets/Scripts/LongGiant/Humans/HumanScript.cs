﻿using UnityEngine;
using System.Collections.Generic;

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

        //GetAllWeaponColliders();

        giantConstruction = gameManager.GetGiantConstruction;
        spawningManager = gameManager.GetHumanSpawningManager;
        attackFrequenceSystem = new FrequenceSystem(1 / timeBetweenTwoAttack);
        attackFrequenceSystem.SetUp(StartAttack);

        if (humanWeapon != null)
        {
            humanWeapon.isKinematic = true;           
        }

        foreach (Collider coll in allWeaponColliders)
            coll.enabled = false;

        if (gameManager.gameFinished)
            Flee();

        if(humanBubbleAnimator != null && !gameManager.gameFinished)
        {
            humanBubbleAnimator.SetInteger("angryCounter", Random.Range(0, 2));
            humanBubbleAnimator.SetTrigger("spawned");
        }
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

        isWalking = false;
        if (humanAnimator != null)
            humanAnimator.SetBool("walking", isWalking);

        if (humanBubbleAnimator != null)
            humanBubbleAnimator.SetTrigger("grabbed");
    }

    public void Flee()
    {
        fleeing = true;
        if (humanWeapon != null)
        {
            humanWeapon.isKinematic = false;
            humanWeapon.transform.parent = GameManager.gameManager.GetAllGameObjectsParent;

            foreach (Collider coll in allWeaponColliders)
                coll.enabled = true;

            Vector3 randomThrowVelocity = Random.onUnitSphere;
            randomThrowVelocity.y = Mathf.Abs(randomThrowVelocity.y);
            randomThrowVelocity = Vector3.Slerp(randomThrowVelocity, Vector3.up, 0.5f);

            humanWeapon.AddForce(randomThrowVelocity * Random.Range(250f, 300f));
            humanWeapon.AddTorque(Random.onUnitSphere * Random.Range(180f, 270f));
        }

        if (humanBubbleAnimator != null)
            humanBubbleAnimator.SetTrigger("fleeing");
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

        if (humanBubbleAnimator != null)
            humanBubbleAnimator.SetTrigger("attack");
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
    [SerializeField] Rigidbody humanWeapon = default;
    [SerializeField] List<Collider> allWeaponColliders = default;
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
    [SerializeField] Transform rendererParent = default;
    [SerializeField] Animator humanAnimator = default;
    [SerializeField] Animator humanBubbleAnimator = default;
        
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
        if (Input.GetKeyDown(KeyCode.T))
        {
            StopAct();
        }

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