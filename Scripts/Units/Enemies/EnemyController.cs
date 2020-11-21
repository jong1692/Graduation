using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Message;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(TargetScanner))]
public class EnemyController : UnitController
{
    [SerializeField]
    protected float attackDelay = 2.0f;
    protected float originAttackDelay;
    [SerializeField]
    protected float idleDelay = 5.0f;
    [SerializeField]
    protected float attackRange = 0.9f;
    [SerializeField]
    protected int numAttackMotion = 1;
    [SerializeField]
    protected int numIdleMotion = 1;
    [SerializeField]
    private int blockingChance = 50;

    protected NavMeshAgent navMeshAgent;
    protected TargetScanner targetScanner;
    protected Vector3 originPos;

    protected float attackTimer;
    protected float idleTimer;

    protected bool isTargetInRange;
    protected bool isReturn;
    protected bool canBlock;

    protected const float timerOffset = 1.0f;

    protected const string randomAttackStr = "RandomAttack";
    protected const string randomIdleStr = "RandomIdle";
    protected const string idleStr = "Idle";
    protected const string targetInRangeStr = "TargetInRange";
    protected const string standUpStr = "StandUp";

    public bool IsTargetInRange
    {
        get { return isTargetInRange; }
        set { isTargetInRange = value; }
    }

    void Start()
    {
        initialize();
    }

    void FixedUpdate()
    {
        calculateHorizontalMovement();
        calculateVerticalMovement();

        if (target == null)
        {
            returnOriginLocation();
            return;
        }

        if (isTargetInRange &&
            (checkCurAnimationWithTag(locomotionStr) || checkCurAnimationWithTag(blockStr))
            || checkCurAnimationWithTag(attackStr))
        {
            setRotation();
            updateRotation();

            chaseTarget();
            block();
        }

        if (checkCanRandIdle())
        {
            randIdle();
        }

        if (checkCanAttack())
        {
            int random = Random.Range(0, numAttackMotion);
            animator.SetInteger(randomAttackStr, random);

            attackDelay = originAttackDelay + Random.Range(-timerOffset, timerOffset);
            attackTimer = 0;

            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }

        attackTimer += Time.deltaTime;
    }

    protected void block()
    {
        if (blockingChance <= Random.Range(0, 100))
        {
            canBlock = false;

            return;
        }

        if (checkCurAnimationWithTag(locomotionStr)
            && target.InAttacking && canBlock)
        {
            canBlock = false;

            inBlocking = true;
            animator.SetBool(inBlockingStr, inBlocking);
        }
        else if (!target.InAttacking)
        {
            canBlock = true;

            inBlocking = false;
            animator.SetBool(inBlockingStr, inBlocking);
        }
    }

    protected override void damaged(Damageable.DamageMessage msg)
    {
        Vector3 direction = msg.damager.transform.position - transform.position;
        direction.y = 0f;

        var localDirection = transform.InverseTransformDirection(direction);

        if (!checkCurAnimationWithTag(attackStr))
            animator.SetTrigger(hurtStr);

        damageable.playAudio(MessageType.DAMAGED);

        animator.SetFloat(hurtFromXStr, localDirection.x);
        animator.SetFloat(hurtFromZStr, localDirection.z);

        if (!isTargetInRange) isTargetInRange = true;
    }

    protected override void moveUnit()
    {
        Vector3 movementVec = Vector3.zero;

        if (isGrounded)
        {
            if (checkCurAnimationWithTag(attackStr))
            {
                movementVec = animator.deltaPosition;
                characterController.Move(movementVec);
            }

            if (checkCurAnimationWithTag(locomotionStr))
                transform.position = navMeshAgent.nextPosition;
            else
                navMeshAgent.Warp(transform.position);
        }
        else
        {
            movementVec = verticalMovementSpeed * transform.up * Time.deltaTime;
            characterController.Move(movementVec);
        }

        characterController.transform.rotation *= animator.deltaRotation;

        checkGrounded();
        animator.SetBool(groundedStr, isGrounded);
        animator.SetBool(targetInRangeStr, isTargetInRange);
    }

    private void randIdle()
    {
        if (idleTimer < idleDelay)
        {
            idleTimer += Time.deltaTime;

            return;
        }

        idleTimer = 0;

        int random = Random.Range(0, numIdleMotion);
        animator.SetInteger(randomIdleStr, random);
        animator.SetTrigger(idleStr);
    }


    public override void endAttack()
    {
        base.endAttack();

        resetAttackTrigger();
    }

    protected override void setRotation()
    {
        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0;
        direction.Normalize();

        targetRotation = Quaternion.LookRotation(direction);
    }

    protected override void updateRotation()
    {
        targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnSpeed * Time.deltaTime);

        transform.rotation = targetRotation;
    }

    public void returnOriginLocation()
    {
        navMeshAgent.SetDestination(originPos);

        navMeshAgent.isStopped = false;

        isReturn = true;
    }

    protected void chaseTarget()
    {
        float distance = (transform.position - target.transform.position).magnitude;
        if (distance < navMeshAgent.stoppingDistance)
        {
            navMeshAgent.isStopped = true;

            animator.SetFloat(movementSpeedStr, 0);
        }
        else
        {
            navMeshAgent.SetDestination(target.transform.position);

            navMeshAgent.isStopped = false;
        }

    }


    protected bool checkCanAttack()
    {
        if (attackTimer < attackDelay || target == null)
        {
            return false;
        }

        float distance = (transform.position - target.transform.position).magnitude;
        if (attackRange > distance && isTargetInRange && !inBlocking)
        {
            return true;
        }

        return false;
    }

    protected bool checkCanRandIdle()
    {
        return !isTargetInRange && !isDied;
    }

    protected override void initialize()
    {
        base.initialize();

        targetScanner = GetComponent<TargetScanner>();

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.isStopped = false;
        navMeshAgent.updateRotation = false;
        navMeshAgent.updatePosition = false;

        originPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        target = PlayerController.Instance.gameObject.GetComponent<UnitController>();

        isTargetInRange = false;
        isReturn = false;
        canBlock = true;

        originAttackDelay = attackDelay;
        attackTimer = attackDelay;

        idleTimer = 0;
        idleDelay += Random.Range(-timerOffset, timerOffset);
    }

    protected override void calculateHorizontalMovement()
    {
        if (isTargetInRange)/*&& checkCanAttack())*/
        {
            targetMovementSpeed = maxMovementSpeed;
            acceleration = groundAcceleration;
            navMeshAgent.acceleration = groundAcceleration;
        }
        else /*if (!isTargetInRange)*/
        {
            targetMovementSpeed = 0;
            acceleration = groundDeceleration;
            navMeshAgent.acceleration = groundDeceleration;
        }

        base.calculateHorizontalMovement();

        navMeshAgent.speed = movementSpeed;
    }

    protected override void die()
    {
        base.die();

        isTargetInRange = false;
    }

    public void spawn()
    {
        animator.SetTrigger(standUpStr);

        damageable.enabled = true;

        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }
}
