using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance
    {
        get { return instance; }
    }

    private static PlayerInput instance;

    private Vector3 movement;
    private Queue<Vector3> movementQueue;
    private Vector3 camMovement;
    private Queue<Vector2> camPositionQueue;

    private bool attack;
    private Queue<bool> attackQueue;
    private bool pause;
    private bool jump;
    private bool roll;
    private bool lockOn;
    private bool block;
    private bool run;


    private bool canDetectInput;
    private bool canSaveData;
    private bool inSaving;
    private bool inLoading;

    private bool skill_01;

    private bool useHpPotion;
    private bool useMpPotion;


    public bool UseHpPotion
    {
        get { return useHpPotion; }
        set { useHpPotion = value; }
    }

    public bool UseMpPotion
    {
        get { return useMpPotion; }
        set { useMpPotion = value; }
    }

    public Queue<bool> AttackQueue
    {
        get { return attackQueue; }
    }

    public bool Skill_01
    {
        get { return skill_01; }
    }

    public Queue<Vector3> MovementQueue
    {
        get { return movementQueue; }
    }

    public Queue<Vector2> CamPositionQueue
    {
        get { return camPositionQueue; }
    }

    public bool CanDetectInput
    {
        get { return canDetectInput; }
        set { canDetectInput = value; }
    }

    public bool CanSaveData
    {
        get { return canSaveData; }
        set { canSaveData = value; }
    }

    public bool IsMoveInput
    {
        get { return !Mathf.Approximately(movement.sqrMagnitude, 0f); }
    }

    public Vector3 Movement
    {
        get { return movement; }
    }

    public Vector3 CamMovement
    {
        get { return camMovement; }
    }

    public bool Attack
    {
        get { return attack; }
    }

    public bool Block
    {
        get { return block; }
    }

    public bool Pause
    {
        get { return pause; }
    }

    public bool Jump
    {
        get { return jump; }
    }

    public bool Roll
    {
        get { return roll; }
    }

    public bool Run
    {
        get { return run; }
    }

    public bool LockOn
    {
        get { return lockOn; }
        set { lockOn = value; }
    }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        canDetectInput = true;
        canSaveData = false;
        inSaving = false;

        movementQueue = new Queue<Vector3>();
        camPositionQueue = new Queue<Vector2>();
        attackQueue = new Queue<bool>();
    }

    void Update()
    {
        if (canDetectInput)
        {
            detectPlayerInput();
        }
        else
        {
            movement = Vector3.zero;
            camMovement = Vector3.zero;
        }

        if (canSaveData && !inSaving)
        {
            StartCoroutine(saveData());
        }
    }

    private IEnumerator saveData()
    {
        inSaving = true;

        while (canSaveData)
        {
            yield return new WaitForSeconds(0.01f);

            movementQueue.Enqueue(movement);
            camPositionQueue.Enqueue(new Vector2(PlayerController.Instance.CameraSetting.FreeLookCamera.m_XAxis.Value,
               PlayerController.Instance.CameraSetting.FreeLookCamera.m_YAxis.Value));

            AttackQueue.Enqueue(attack);
        }

        inSaving = false;

        StartCoroutine(loadData());
    }

    private IEnumerator loadData()
    {
        inLoading = true;

        while (movementQueue.Count != 0)
        {
            yield return new WaitForSeconds(0.001f);

            movementQueue.Dequeue();
            camPositionQueue.Dequeue();
            AttackQueue.Dequeue();
        }

        inLoading = false;
    }

    public bool getKeyDown(KeyCode keyCode)
    {
        return Input.GetKeyDown(keyCode);
    }

    private void detectPlayerInput()
    {
        movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        camMovement = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));

        jump = Input.GetButtonDown("Jump");
        pause = Input.GetButtonDown("Pause");
        roll = Input.GetButtonDown("Roll");
        lockOn = Input.GetButtonDown("LockOn");
        block = Input.GetMouseButton(1);
        run = Input.GetButton("Run");
        skill_01 = Input.GetButton("Skill_01");

        useHpPotion = Input.GetButtonDown("UseHpPotion");
        //useMpPotion = Input.GetButtonDown("UseMpPotion");

        PlayerController playerController =  PlayerController.Instance;

        if (playerController.checkCurAnimationWithTag("Block")
            || playerController.checkCurAnimationWithTag("Idle")
            || playerController.checkCurAnimationWithTag("Locomotion")
            || (playerController.checkCurAnimationWithTag("Attack") 
            && PlayerController.Instance.AnimStateTime > 0.3f
            && PlayerController.Instance.AnimStateTime < 0.9f))
            attack = Input.GetButtonDown("Attack");
        else
            attack = false;

    }
}
