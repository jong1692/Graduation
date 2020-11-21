using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Weapon;
using ObjectTypes;
using UnityEngine.Events;

public class TrapController : MonoBehaviour
{
    [Serializable]
    protected struct AttackPoint
    {
        public float radius;
        public Transform attackRoot;
    }

    [SerializeField]
    private int damage;
    [SerializeField]
    private AttackPoint[] attackPoints;
    [SerializeField]
    private LayerMask targetLayerMask;
    [SerializeField]
    private ParticleType particleType = ParticleType.HIT;
    [SerializeField]
    private UnityEvent startEvent;

    private Action action;

    private Vector3[] prevPosition;
    private RaycastHit[] raycastHitBuffers;
    private List<GameObject> damagedObjectsList;

    private Damageable.DamageMessage damageMessage;

    private bool canRotate = false;
    private bool canSwing = false;
    private bool inAttacking = false;

    private Vector3 targetVec;
    private Vector3 originVec;

    private float swingSpeed = 100.0f;

    const int hitBufferSize = 32;

    void Start()
    {
        prevPosition = new Vector3[attackPoints.Length];
        raycastHitBuffers = new RaycastHit[hitBufferSize];
        damagedObjectsList = new List<GameObject>();

        damageMessage = new Damageable.DamageMessage();
        damageMessage.damageAmount = damage;
        damageMessage.damager = this.gameObject;
        damageMessage.weapon = this.gameObject;
        damageMessage.particleType = particleType;
        damageMessage.weaponType = WeaponType.EMPTY_HAND;
        damageMessage.canBlock = false;

        beginAttack();

        action += startEvent.Invoke;
        action();

        originVec = transform.position;

        InvokeRepeating("clearDamagedList", 0, 1.0f);
    }

    void Update()
    {
        if (inAttacking)
        {
            inAttack();
        }

        if (canRotate) rotate();
    }

    public void rotate()
    {
        transform.Rotate(-180.0f * Time.deltaTime, 0, 0);
    }

    public void repeatingTrust(float targetPosY)
    {
        targetVec.y = originVec.y + targetPosY;

        InvokeRepeating("trust", 0, 4.0f);
    }

    private void trust()
    {
        StopAllCoroutines();
        StartCoroutine(trustCoroutine());
    }

    IEnumerator trustCoroutine()
    {
        while (transform.position.y < targetVec.y)
        {
            Vector3 vec = transform.position;
            vec.y += 5.0f * Time.deltaTime;

            transform.position = vec;

            yield return null;
        }

        while (transform.position.y > originVec.y)
        {
            Vector3 vec = transform.position;
            vec.y -= 2.0f * Time.deltaTime;

            transform.position = vec;

            yield return null;
        }

    }

    public void beginRotate()
    {
        canRotate = true;
    }

    public void beginSwing(float targetPos)
    {
        canSwing = true;

        this.targetVec.z = targetPos;
    }

    public void beginAttack()
    {
        inAttacking = true;

        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            prevPosition[idx] = attackPoints[idx].attackRoot.position;
        }
    }

    public void endAttack()
    {
        inAttacking = false;

        if (damagedObjectsList.Count != 0)
        {
            damagedObjectsList.Clear();
        }
    }

    public void clearDamagedList()
    {
        if (damagedObjectsList.Count != 0)
            damagedObjectsList.Clear();
    }

    private void inAttack()
    {
        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            Vector3 curPositon = attackPoints[idx].attackRoot.position;
            Vector3 attackVector = curPositon - prevPosition[idx];

            Ray ray = new Ray(curPositon, transform.forward);
            int contacts = Physics.SphereCastNonAlloc
                (ray, attackPoints[idx].radius, raycastHitBuffers, attackVector.magnitude, targetLayerMask);

            for (int i = 0; i < contacts; i++)
            {
                Collider collider = raycastHitBuffers[i].collider;
                checkDamage(collider, attackPoints[idx]);

            }

            prevPosition[idx] = curPositon;
        }
    }

    private void checkDamage(Collider collider, AttackPoint attackPoint)
    {
        Damageable damageableScript = collider.GetComponent<Damageable>();
        GameObject gameObject = collider.gameObject;

        if (damageableScript != null && !damagedObjectsList.Contains(gameObject))
        {
            if (damageableScript.Invincibility) return;

            damageMessage.damageSource = attackPoint.attackRoot.position;
            damageMessage.radius = attackPoint.radius;

            damagedObjectsList.Add(gameObject);
            damageableScript.applyDamage(damageMessage);
        }
    }
}
