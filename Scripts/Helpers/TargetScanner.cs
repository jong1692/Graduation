using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScanner : MonoBehaviour
{
    [SerializeField]
    private float detectionDistance = 10f;
    [SerializeField]
    private float detectionRange = 120f;
    [SerializeField]
    private AudioSource detectAudioSource;

    private bool isDetect;
    private GameObject target;

    private EnemyController enemyController;

    const float returnAmount = 2.5f;

    public bool IsDetect
    {
        get { return isDetect; }
        set { isDetect = value; }
    }

    void Start()
    {
        initialize();
    }

    void Update()
    {
        if (isDetect && target.gameObject != null)
        {
            detectTarget();
        }
        else if (target.gameObject != null)
        {
            float distance = (target.transform.position - transform.position).magnitude;
            if (distance > detectionDistance * returnAmount)
            {
                enemyController.IsTargetInRange = false;

                isDetect = true;

                return;
            }
        }
    }

    private void initialize()
    {
        isDetect = true;

        target = PlayerController.Instance.gameObject;
        enemyController = GetComponent<EnemyController>();
    }

    private void detectTarget()
    {
        Vector3 playerPos = PlayerController.Instance.transform.position + Vector3.up * 1.0f;
        Vector3 heading = playerPos - transform.position;

        if (heading.sqrMagnitude > detectionDistance * detectionDistance) return;

        if (enemyController.checkCurAnimationWithTag("Hurt"))
        {
            enemyController.IsTargetInRange = true;

            if (detectAudioSource != null)
                detectAudioSource.Play();

            isDetect = false;

            return;
        }

        if (Vector3.Dot(heading.normalized, transform.forward) <
      Mathf.Cos(detectionRange * 0.5f * Mathf.Deg2Rad))
            return;

        RaycastHit raycastHit;
        Physics.Raycast(transform.position, playerPos - transform.position, out raycastHit, detectionDistance);

        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.gameObject == target)
            {
                enemyController.IsTargetInRange = true;

                isDetect = false;
            }
        }
    }

    public bool checkTargetInRange(GameObject target, float distance)
    {
        RaycastHit raycastHit;
        Physics.Raycast(transform.position, target.transform.position + Vector3.up * 1.0f - transform.position, out raycastHit, distance);

        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.gameObject == target)
            {
                return true;
            }
        }

        return false;
    }
}