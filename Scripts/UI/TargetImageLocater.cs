using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetImageLocater : MonoBehaviour
{
    private static TargetImageLocater instance;

    [SerializeField]
    private Transform lockOnImage;

    private Transform target;
    private UnitController targetUnitController;


    public static TargetImageLocater Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance == null) instance = this;

        target = null;
    }

    void Update()
    {
        if (target != null)
        {
            locateTargetImage();
        }
    }

    private void locateTargetImage()
    {
        Vector3 screen = Camera.main.WorldToScreenPoint(target.position);

        lockOnImage.transform.position = screen;
    }

    public void setTarget(GameObject targetObj)
    {
        lockOnImage.gameObject.SetActive(true);

        target = targetObj.transform.Find("CenterTarget");

        targetUnitController = lockOnImage.gameObject.GetComponent<UnitController>();
    }

    public void resetTarget()
    {
        target = null;
        targetUnitController = null;

        lockOnImage.gameObject.SetActive(false);
    }
}
