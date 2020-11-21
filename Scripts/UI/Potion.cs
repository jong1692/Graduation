using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Potion : MonoBehaviour
{
    enum POTION_TYPE
    {
        HP, MP
    }

    [SerializeField]
    private POTION_TYPE potionType;

    [SerializeField]
    private Sprite[] sprites;

    private Image image;

    void Start()
    {
        image = GetComponent<Image>();
    }

    void Update()
    {
        float amount;

        if (potionType == POTION_TYPE.HP)
            amount = PlayerController.Instance.CurHpPotionAmount;
        else
            amount = PlayerController.Instance.CurMpPotionCnt;

        image.fillAmount = amount;
    }
}
