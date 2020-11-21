using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : UnitController
{
    private static PlayerController instance;

    [SerializeField]
    private AudioSource locomotionAudioSource;
    [SerializeField]
    private AudioSource skillAudioSource;
    [SerializeField]
    private float lockOnRadius = 7.0f;
    [SerializeField]
    private LayerMask lockOnLayerMask;
    [SerializeField]
    private GameObject ultimateObject;
    [SerializeField]
    private float skill_01_CoolTime = 10.0f;

    private float skill_01_Timer;

    private float hpPotionTimer;
    private int curHpPotionCnt;

    private float mpPotionTimer;
    private int curMpPotionCnt;

    private const float potionCooltime = 5.0f;
    private const int maxPotionCnt = 4;
    private const int increaseAmount = 200;

    private RaycastHit[] raycastHitBuffers;

    private PlayerInput playerInput;
    private CameraSetting cameraSetting;

    private Vector3 movement;
    private Vector3 originPosition;
    private Quaternion originRotation;

    private Vector2 originCamPosition;
    private Quaternion originCamRotation;

    const int hitBufferSize = 32;

    public float Skill_01_Amount
    {
        get { return skill_01_Timer / skill_01_CoolTime; }
    }

    public float CurHpPotionAmount
    {
        get { return hpPotionTimer / potionCooltime; }
    }


    public int CurHpPotionCnt
    {
        get { return curHpPotionCnt; }
    }

    public int CurMpPotionCnt
    {
        get { return curMpPotionCnt; }
    }

    public CameraSetting CameraSetting
    {
        get { return cameraSetting; }
        set { cameraSetting = value; }
    }

    public static PlayerController Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        initialize();
    }

    void FixedUpdate()
    {
        calculateHorizontalMovement();
        calculateVerticalMovement();

        detectPlayerInput();

        caculateTimer();
    }

    private void detectPlayerInput()
    {
        if (playerInput.Movement != Vector3.zero && !isDied && !inAttacking && !checkCurAnimationWithTag(hurtStr))
        {
            setRotation();
            updateRotation();
        }

        if (playerInput.Attack)
        {
            StopCoroutine(beginAttackMotion());
            StartCoroutine(beginAttackMotion());
        }

        if (playerInput.Roll && !isDied)
        {
            StartCoroutine(beginRollMotion());
        }

        if (playerInput.Block)
        {
            beginBlock();
        }
        else if (!inLockOn)
        {
            endBlock();
        }

        if (damageable.CurMpPoint <= 0)
        {
            resetTarget();
            endBlock();
        }

        if (playerInput.LockOn)
        {
            setTarget();
        }

        if (playerInput.Skill_01)
        {
            useSkill();
        }

        if (playerInput.UseHpPotion)
        {
            useHpPotion();
        }

        if (playerInput.UseMpPotion)
        {
            useMpPotion();
        }

        if (target != null && target.IsDied)
            resetTarget();
    }

    private void caculateTimer()
    {
        if (skill_01_Timer < skill_01_CoolTime)
            skill_01_Timer += Time.deltaTime;

        if (hpPotionTimer < potionCooltime)
        {
            hpPotionTimer += Time.deltaTime;
        }

        if (mpPotionTimer < potionCooltime && curMpPotionCnt < maxPotionCnt)
        {
            mpPotionTimer += Time.deltaTime;
        }
        else if (mpPotionTimer > potionCooltime && curMpPotionCnt < maxPotionCnt)
        {
            curMpPotionCnt++;
            mpPotionTimer = 0;
        }
    }

    public void useSkill()
    {
        if (skill_01_Timer < skill_01_CoolTime) return;

        animator.SetBool(useFirstSkillStr, true);

        skill_01_Timer = 0;

        skillAudioSource.Play();
    }

    public void spawnUltimateObject()
    {
        GameObject ultimate = Instantiate(ultimateObject, transform.position, Quaternion.identity, transform);

        ultimate.transform.GetChild(0).GetComponent<WeaponController>().beginAttack();

        animator.SetBool(useFirstSkillStr, false);
        animator.ResetTrigger(hurtStr);

        Destroy(ultimate, 1.5f);
    }

    public void lying()
    {
        animator.SetTrigger("Lying");
    }

    private IEnumerator beginRollMotion()
    {
        animator.SetTrigger(rollStr);

        inRolling = true;
        animator.SetBool(inRollingStr, inRolling);

        damageable.Invincibility = true;

        while (!checkCurAnimationWithTag(rollStr))
        {
            yield return null;
        }

        while (checkCurAnimationWithTag(rollStr))
        {
            animator.SetFloat(stateTimeStr, animator.GetCurrentAnimatorStateInfo(0).normalizedTime);

            yield return null;
        }

        inRolling = false;
        animator.SetBool(inRollingStr, inRolling);

        damageable.Invincibility = false;
    }

    private void useHpPotion()
    {
        if (hpPotionTimer < potionCooltime) return;

        damageable.increaseHitPoint(increaseAmount);

        hpPotionTimer = 0;
    }


    private void useMpPotion()
    {
        if (curMpPotionCnt <= 0) return;

        playerInput.UseMpPotion = false;
        curMpPotionCnt--;

        damageable.increaseManaPoint(increaseAmount);
    }

    protected override void damaged(Damageable.DamageMessage msg)
    {
        base.damaged(msg);

        var cameraShakeScript = cameraSetting.FreeLookCamera.GetComponent<CameraShake>();
        StartCoroutine(cameraShakeScript.cameraShake(0.25f, 0.4f));
    }

    protected override void initialize()
    {
        if (instance == null) instance = this;

        base.initialize();

        playerInput = GetComponent<PlayerInput>();

        raycastHitBuffers = new RaycastHit[hitBufferSize];

        skill_01_Timer = skill_01_CoolTime;

        hpPotionTimer = potionCooltime;
    }

    protected override void calculateHorizontalMovement()
    {
        movement = playerInput.Movement;

        movement.Normalize();

        targetMovementSpeed = movement.magnitude * maxMovementSpeed;

        if (playerInput.IsMoveInput)
        {
            acceleration = groundAcceleration;
        }
        else
        {
            acceleration = groundDeceleration;
        }

        if (damageable.CurMpPoint > 0)
        {
            animator.SetBool(inRunningStr, playerInput.Run);
        }
        else
        {
            animator.SetBool(inRunningStr, false);
        }

        base.calculateHorizontalMovement();
    }

    protected override void moveUnit()
    {
        base.moveUnit();

        if (checkCurAnimationWithTag(locomotionStr) && !locomotionAudioSource.isPlaying)
        {
            locomotionAudioSource.Play();
        }
        else if (!checkCurAnimationWithTag(locomotionStr))
        {
            locomotionAudioSource.Stop();
        }
    }

    protected override void calculateVerticalMovement()
    {
        base.calculateVerticalMovement();

        if (isGrounded && playerInput.Jump && !inAttacking)
        {
            verticalMovementSpeed = jumpPower;
            isGrounded = false;
        }
    }

    protected void resetTarget()
    {
        target = null;

        TargetImageLocater.Instance.resetTarget();

        inLockOn = false;
        animator.SetBool(inLockOnStr, inLockOn);

        cameraSetting.changeFreeCamera();
    }

    protected void beginBlock()
    {
        inBlocking = true;
        animator.SetBool(inBlockingStr, inBlocking);
    }

    protected void endBlock()
    {
        inBlocking = false;
        animator.SetBool(inBlockingStr, inBlocking);
    }

    protected void setTarget()
    {
        playerInput.LockOn = false;

        if (target != null)
        {
            resetTarget();
            endBlock();

            return;
        }

        Ray ray = new Ray(transform.position, Vector3.up);
        int contacts = Physics.SphereCastNonAlloc
            (ray, lockOnRadius, raycastHitBuffers, 1, lockOnLayerMask);

        GameObject targetObj = null;
        float distance = Mathf.Infinity;
        for (int i = 0; i < contacts; i++)
        {
            Transform objTransform = raycastHitBuffers[i].transform;

            if (objTransform.GetComponent<UnitController>().IsDied == true) continue;

            if (distance > (transform.position - objTransform.position).sqrMagnitude)
            {
                distance = (transform.position - objTransform.position).sqrMagnitude;
                targetObj = objTransform.gameObject;
            }
        }
        if (targetObj == null) return;

        target = targetObj.GetComponent<UnitController>();

        TargetImageLocater.Instance.setTarget(targetObj);

        inLockOn = true;

        animator.SetBool(inLockOnStr, inLockOn);

        cameraSetting.changeFiexedCamera(target.transform);

        beginBlock();
        setRotation();
    }

    protected override void updateRotation()
    {
        Vector3 localInput = new Vector3(movement.x, 0f, movement.z);

        float groundedTurnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, movementSpeed / targetMovementSpeed);
        targetRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, groundedTurnSpeed * Time.deltaTime);

        transform.rotation = targetRotation;
    }

    protected override void setRotation()
    {
        Vector3 movementDirection = playerInput.Movement;
        movementDirection.Normalize();

        animator.SetFloat("MovementX", movementDirection.x);
        animator.SetFloat("MovementZ", movementDirection.z);

        if (target != null || inBlocking)
        {
            Vector3 direction;

            if (target != null) direction = target.transform.position - transform.position;
            else direction = transform.forward;

            direction.y = 0;
            direction.Normalize();

            targetRotation = Quaternion.LookRotation(direction);

            return;
        }

        Vector3 forward = Quaternion.Euler(0f, cameraSetting.FreeLookCamera.m_XAxis.Value, 0f) * Vector3.forward;
        forward.y = 0f;
        forward.Normalize();

        Quaternion rotation;
        if (Mathf.Approximately(Vector3.Dot(movementDirection, Vector3.forward), -1.0f))
        {
            rotation = Quaternion.LookRotation(-forward);
        }
        else
        {
            Quaternion cameraToInputOffset = Quaternion.FromToRotation(Vector3.forward, movementDirection);

            rotation = Quaternion.LookRotation(cameraToInputOffset * forward);
        }
        targetRotation = rotation;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Dialogue")
        {
            hit.gameObject.GetComponent<DialogueLoader>().loadAndShowDialogue();
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "InteractObject")
        {
            other.gameObject.GetComponent<ObjectController>().showInteractImage();
        }
    }
}
