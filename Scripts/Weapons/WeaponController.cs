using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Weapon;
using ObjectTypes;
using UnityEngine.Events;

public class WeaponController : MonoBehaviour
{
    [Serializable]
    private struct AttackPoint
    {
        public float radius;
        public Transform attackRoot;
    }

    [SerializeField]
    private WeaponType weaponType;
    [SerializeField]
    private int damage;
    [SerializeField]
    private AttackPoint[] attackPoints;
    [SerializeField]
    private Transform crashAttackPointRoot;
    [SerializeField]
    private LayerMask targetLayerMask;
    [SerializeField]
    private LayerMask crashLayerMask;
    [SerializeField]
    private ParticleType particleType;
    [SerializeField]
    private bool canBlock = true;
    [SerializeField]
    private UnityEvent onCrash;
    [SerializeField]
    private UnityEvent onStart;

    protected GameObject owner;
    private Vector3[] prevPosition;
    private RaycastHit[] raycastHitBuffers;
    private List<GameObject> damagedObjectsList;
    private Damageable.DamageMessage damageMessage;
    private AudioSource audioSource;
    private ParticleSystem waeponParticle;

    private Action action;
    private Action startAction;

    private bool inMeleeAttacking;
    private bool inRangeAttacking;

    const int hitBufferSize = 32;

    public GameObject Owner
    {
        get { return owner; }
        set { Owner = value; }
    }

    private void Awake()
    {

        initialize();
    }

    private void initialize()
    {
        GameObject parent = transform.parent.gameObject;
        while (parent.transform.tag != "Player" && parent.transform.tag != "Enemy")
        {
            parent = parent.transform.parent.gameObject;
        }

        if (parent != null)
            owner = parent;

        audioSource = GetComponent<AudioSource>();

        if (weaponType == WeaponType.BREATH)
            waeponParticle = GetComponentInChildren<ParticleSystem>();

        prevPosition = new Vector3[attackPoints.Length];
        raycastHitBuffers = new RaycastHit[hitBufferSize];
        damagedObjectsList = new List<GameObject>();

        damageMessage = new Damageable.DamageMessage();
        damageMessage.damageAmount = damage;
        damageMessage.damager = owner;
        damageMessage.weapon = this.gameObject;
        damageMessage.particleType = particleType;
        damageMessage.weaponType = weaponType;
        damageMessage.canBlock = canBlock;

        startAction += onStart.Invoke;
        startAction();
    }

    public void beginAttack()
    {
        switch (weaponType)
        {
            case WeaponType.ONE_HAND_RANGE:
                rangeAttack();

                break;

            case WeaponType.TWO_HAND_RANGE:
                rangeAttack();

                break;

            default:
                meleeAttack();
                break;

        }
    }

    private void meleeAttack()
    {
        inMeleeAttacking = true;

        if (waeponParticle != null)
            waeponParticle.Play();

        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            prevPosition[idx] = attackPoints[idx].attackRoot.position;
        }
    }

    private void rangeAttack()
    {
        if (particleType == ParticleType.NULL) return;

        Transform target = owner.GetComponent<UnitController>().Target.transform.Find("CenterTarget");

        ParticleSystem projectile = ObjectManager.Instance.getParticle(particleType);

        projectile.gameObject.SetActive(true);
        projectile.transform.position = transform.position;
        projectile.GetComponent<Projectile>().shootProjectile(target.position, owner, damage);
        projectile.Play();
    }

    public void endAttack()
    {
        inMeleeAttacking = false;
        inRangeAttacking = false;

        if (waeponParticle != null)
            waeponParticle.Stop();

        damagedObjectsList.Clear();
    }

    void FixedUpdate()
    {
        if (inMeleeAttacking)
        {
            inMeleeAttack();
        }
    }

    private void inMeleeAttack()
    {

        for (int idx = 0; idx < attackPoints.Length; idx++)
        {
            Vector3 curPositon = attackPoints[idx].attackRoot.position;
            Vector3 attackVector = curPositon - prevPosition[idx];

            Ray ray = new Ray(curPositon, attackVector.normalized);
            int contacts = Physics.SphereCastNonAlloc
                (ray, attackPoints[idx].radius, raycastHitBuffers, attackVector.magnitude, targetLayerMask);

            for (int i = 0; i < contacts; i++)
            {
                Collider collider = raycastHitBuffers[i].collider;
                checkDamage(collider, attackPoints[idx]);
            }

            contacts = Physics.SphereCastNonAlloc
               (ray, attackPoints[idx].radius, raycastHitBuffers, attackVector.magnitude, crashLayerMask);
            for (int i = 0; i < contacts; i++)
            {
                Collider collider = raycastHitBuffers[i].collider;
                crash(collider, attackPoints[idx]);

                if (attackPoints[idx].attackRoot == crashAttackPointRoot)
                {
                    owner.GetComponent<UnitController>().setCrashTrigger();
                    inMeleeAttacking = false;
                    return;
                }

                break;
            }


            prevPosition[idx] = curPositon;
        }
    }

    private void crash(Collider collider, AttackPoint attackPoint)
    {
        if (!damagedObjectsList.Contains(gameObject))
        {
            damagedObjectsList.Add(gameObject);

            if (audioSource)
                audioSource.Play();

            StartCoroutine(owner.GetComponent<Damageable>().
                locateCrashParticle(attackPoint.attackRoot.position, owner.transform.position));

            action += onCrash.Invoke;
            action();
        }
    }

    private void checkDamage(Collider collider, AttackPoint attackPoint)
    {
        Damageable damageableScript = collider.GetComponent<Damageable>();
        GameObject gameObject = collider.gameObject;

        if (damageableScript != null && !damagedObjectsList.Contains(gameObject))
        {
            if (damageableScript.Invincibility || collider.gameObject == owner) return;

            damageMessage.damageSource = attackPoint.attackRoot.position;
            damageMessage.radius = attackPoint.radius;

            damagedObjectsList.Add(gameObject);
            damageableScript.applyDamage(damageMessage);
        }
    }
}
