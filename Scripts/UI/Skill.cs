using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    enum SKILL_TYPE
    {
        FIRST, SECOND
    }

    [SerializeField]
    private SKILL_TYPE skillType;

    private Image skillImage;

    void Start()
    {
        skillImage = GetComponent<Image>();

    }

    // Update is called once per frame
    void Update()
    {
        float amount;

        if (skillType == SKILL_TYPE.FIRST)
            amount = PlayerController.Instance.Skill_01_Amount;
        else
            amount = PlayerController.Instance.Skill_01_Amount;

        skillImage.fillAmount = amount;
    }
}
