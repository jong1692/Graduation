using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Message;
using UnityEngine.UI;
using ObjectTypes;
using Weapon;

public class Damageable : MonoBehaviour
{
    public struct DamageMessage
    {
        public int damageAmount;
        public GameObject damager;
        public GameObject weapon;
        public Vector3 damageSource;

        public float radius;
        public bool canBlock;

        public ParticleType particleType;
        public WeaponType weaponType;
    }

    [SerializeField]
    private int maxHitPoint = 100;
    private int curHitPoint;

    [SerializeField]
    private int maxMpPoint = 100;
    private float curMpPoint;

    [SerializeField]
    private float minStartTime = 4.0f;
    [SerializeField]
    private float maxStartTime = 6.0f;
    [SerializeField]
    private float delay = 3.0f;
    [SerializeField]
    private Material diffuseMaterial;
    [SerializeField]
    private UnityEvent onDeath;
    [SerializeField]
    private Transform hpBar;
    [SerializeField]
    private Transform shieldBar;
    [SerializeField]
    private AudioSource hitAudioSource;
    [SerializeField]
    private AudioSource blockAudioSource;

    private float destroyTimer;
    private float destroyStartTime;
    private bool invincibility = false;

    private ParticleSystem particle;
    private Renderer modelRenderer;
    private Action action;
    private UnitController unitController;

    private Transform ownerTransform;

    const string dissolveParticlesStr = "DissolveParticles";

    public int CurHitPoint
    {
        get { return curHitPoint; }
    }

    public float CurMpPoint
    {
        get { return curMpPoint; }
    }


    public bool Invincibility
    {
        get { return invincibility; }
        set { invincibility = value; }
    }

    void Awake()
    {
        initialize();
    }

    void Update()
    {
        if (hitAudioSource != null)
        {
            if (hitAudioSource.isPlaying)
                hitAudioSource.pitch = Time.timeScale;
        }

        if (shieldBar != null)
        {
            if (PlayerInput.Instance.Run)
            {
                curMpPoint -= 5 * Time.deltaTime;

                if (curMpPoint < 0) curMpPoint = 0;
            }
            else
            {
                curMpPoint += 5 * Time.deltaTime;

                if (curMpPoint > maxMpPoint) curMpPoint = maxMpPoint;
            }
            shieldBar.GetChild(0).GetComponent<Image>().fillAmount = curMpPoint / (float)maxMpPoint;
        }

        if (hpBar == null || GetComponent<PlayerController>() != null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(ownerTransform.position);
        if (screenPos.z < 0)
        {
            hpBar.gameObject.SetActive(false);

            return;
        }
        else if (hpBar.gameObject.activeInHierarchy == false)
        {
            hpBar.gameObject.SetActive(true);
        }

        hpBar.transform.position = screenPos;
    }

    private void initialize()
    {
        curHitPoint = maxHitPoint;
        if (hpBar != null)
            hpBar.GetChild(0).GetComponent<Image>().fillAmount = curHitPoint / (float)maxHitPoint;

        curMpPoint = maxMpPoint;
        if (shieldBar != null)
            shieldBar.GetChild(0).GetComponent<Image>().fillAmount = curMpPoint / (float)maxMpPoint;

        destroyStartTime = UnityEngine.Random.Range(minStartTime, maxStartTime);
        destroyTimer = 0;

        particle = transform.Find(dissolveParticlesStr).GetComponent<ParticleSystem>();
        modelRenderer = GetComponentInChildren<Renderer>();

        unitController = GetComponent<UnitController>();

        ownerTransform = transform.Find("HeadTarget");
    }

    public IEnumerator destroyUnit()
    {
        action += onDeath.Invoke;
        action();

        if (hpBar != null)
        {
            hpBar.gameObject.SetActive(false);
            hpBar = null;
        }

        particle.gameObject.SetActive(true);
        if (!particle.isPlaying)
            particle.Play();

        while (destroyTimer < destroyStartTime)
        {
            destroyTimer += Time.deltaTime;
            yield return null;
        }

        destroyTimer = 0;
        modelRenderer.material = diffuseMaterial;

        while (destroyTimer < delay)
        {
            destroyTimer += Time.deltaTime;

            Color color = modelRenderer.material.color;
            color.a = Mathf.Lerp(color.a, 0, destroyTimer / delay);

            modelRenderer.material.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }

    public void increaseHitPoint(int amount)
    {
        curHitPoint += amount;

        if (curHitPoint > maxHitPoint) curHitPoint = maxHitPoint;

        if (hpBar != null)
            hpBar.GetChild(0).GetComponent<Image>().fillAmount = curHitPoint / (float)maxHitPoint;
    }

    public void increaseManaPoint(int amount)
    {
        curMpPoint += amount;

        if (curMpPoint > maxMpPoint) curMpPoint = maxMpPoint;
    }

    public IEnumerator locateCrashParticle(Vector3 position, Vector3 lookAtPos)
    {
        ParticleSystem crashParticle = ObjectManager.Instance.getParticle(ParticleType.CRASH);

        crashParticle.transform.position = position;
        crashParticle.transform.LookAt(lookAtPos);
        crashParticle.gameObject.SetActive(true);
        crashParticle.Play();

        crashParticle.GetComponentInChildren<Light>().enabled = true;

        yield return new WaitForSeconds(0.02f);

        crashParticle.GetComponentInChildren<Light>().enabled = false;

        while (crashParticle.isPlaying)
        {
            yield return null;
        }

        crashParticle.gameObject.SetActive(false);
    }

    public IEnumerator locateHitParticle(Vector3 position, DamageMessage msg)
    {
        ParticleSystem hitParticle = ObjectManager.Instance.getParticle(msg.particleType);

        if (msg.weaponType == WeaponType.BREATH)
        {
            Transform target = msg.damager.GetComponent<UnitController>().Target.transform;
            SkinnedMeshRenderer skinnedMeshRenderer = target.GetComponentInChildren<SkinnedMeshRenderer>();

            var shape = hitParticle.shape;
            shape.skinnedMeshRenderer = skinnedMeshRenderer;
        }

        hitParticle.transform.position = position;
        hitParticle.transform.LookAt(msg.damageSource);
        hitParticle.gameObject.SetActive(true);
        hitParticle.Play();

        while (hitParticle.isPlaying)
        {
            yield return null;
        }

        hitParticle.gameObject.SetActive(false);
    }

    public void playAudio(MessageType type)
    {
        if (type == MessageType.DAMAGED && hitAudioSource != null)
            hitAudioSource.Play();

        if (type == MessageType.BLOCKED && blockAudioSource != null)
            blockAudioSource.Play();
    }

    private bool checkBlocked(Vector3 localDirection, DamageMessage msg)
    {
        return localDirection.z > 0f && unitController.checkCurAnimationWithTag("Block") && msg.canBlock;
    }

    public void applyDamage(DamageMessage msg)
    {
        Vector3 direction = msg.damager.transform.position - transform.position;
        direction.y = 0f;

        var localDirection = transform.InverseTransformDirection(direction);
        if (checkBlocked(localDirection, msg))
        {
            curMpPoint -= msg.damageAmount;

            if (shieldBar)
            {
                shieldBar.GetChild(0).GetComponent<Image>().fillAmount = curMpPoint / (float)maxMpPoint;
            }

            Damageable.DamageMessage damageMessage = new Damageable.DamageMessage();
            damageMessage.damageAmount = 0;
            damageMessage.damager = gameObject;
            damageMessage.damageSource = transform.position;

            msg.damager.GetComponent<UnitController>().receiveMessage(MessageType.CRASHED, this, damageMessage);

            playAudio(MessageType.BLOCKED);

            Vector3 hitPos = Vector3.Lerp(unitController.CenterTarget.position, msg.weapon.transform.position, 0.6f);
            hitPos.y = unitController.CenterTarget.position.y;

            unitController.receiveMessage(MessageType.BLOCKED, this, msg);
            StartCoroutine(locateCrashParticle(hitPos, msg.damager.transform.position));
        }
        else
        {
            curHitPoint -= msg.damageAmount;

            if (hpBar == null)
            {
                hpBar = ObjectManager.Instance.getObject(ObjectType.HP_BAR).GetComponent<Image>().transform;
                hpBar.gameObject.SetActive(true);
            }

            StartCoroutine(slowMotion());

            Vector3 hitPos = Vector3.Lerp(unitController.CenterTarget.position, msg.damageSource, 0.5f);

            if (msg.particleType != ParticleType.NULL)
                StartCoroutine(locateHitParticle(hitPos, msg));

            hpBar.GetChild(0).GetComponent<Image>().fillAmount = curHitPoint / (float)maxHitPoint;

            MessageType messageType = curHitPoint <= 0 ? MessageType.DEAD : MessageType.DAMAGED;
            unitController.receiveMessage(messageType, this, msg);

            if (messageType == MessageType.DEAD)
                StartCoroutine(destroyUnit());
        }
    }

    private IEnumerator slowMotion()
    {
        Time.timeScale = 0.001f;

        yield return new WaitForSeconds(Time.timeScale * 0.01f);

        Time.timeScale = 1f;
    }
}
