using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectonArrow : MonoBehaviour
{

    private Transform target;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target);
        }
    }

    public void setTarget(Transform trans)
    {
        target = trans;
    }
}
