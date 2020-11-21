using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EelectricSaw : WeaponController
{
    [SerializeField]
    private Transform sawBladeTransform;
    [SerializeField]
    private AudioSource audioSource;

    private const float rotateSpeed = -300.0f;

    void Update()
    {
        EnemyController enemyController = owner.GetComponent<EnemyController>();
        if (enemyController != null && enemyController.IsTargetInRange == true)
        {
            workSaw();
            
            if (audioSource != null && audioSource.isPlaying == false)
                audioSource.Play();
        }
        else if (enemyController != null && enemyController.IsTargetInRange == false)
        {
            if (audioSource != null && audioSource.isPlaying == true)
                audioSource.Stop();
        }
    }


    private void workSaw()
    {
        sawBladeTransform.Rotate(new Vector3(rotateSpeed, 0, 0) * Time.deltaTime);
    }
}
