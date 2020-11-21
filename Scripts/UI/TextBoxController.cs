using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxController : MonoBehaviour
{
    [SerializeField]
    private Vector3 targetPos;

    [SerializeField]
    private float moveSpeed = 5;

    [SerializeField]
    private Text missionText;

    [SerializeField]
    private Text mainText;

    private Vector3 originPos;

    void Start()
    {
        originPos = transform.position;
    }

    void Update()
    {

    }

    public void show()
    {
        StartCoroutine(moveToShow());
    }

    private IEnumerator moveToShow()
    {
        if (missionText != null)
        {
            missionText.text = mainText.text;
        }

        float distacne = Vector3.Distance(targetPos, transform.position);
        while (!Mathf.Approximately(distacne, 0))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            distacne = Vector3.Distance(targetPos, transform.position);

            yield return null;
        }
    }

    public void hide()
    {
        StartCoroutine(moveToHide());
    }

    public void hideAndShow()
    {
        StartCoroutine(moveToHideAndShow());
    }


    private IEnumerator moveToHide()
    {
        float distacne = Vector3.Distance(originPos, transform.position);
        while (!Mathf.Approximately(distacne, 0))
        {
            transform.position = Vector3.MoveTowards(transform.position, originPos, moveSpeed * Time.deltaTime);
            distacne = Vector3.Distance(originPos, transform.position);

            yield return null;
        }
    }

    private IEnumerator moveToHideAndShow()
    {
        float distacne = Vector3.Distance(originPos, transform.position);
        while (!Mathf.Approximately(distacne, 0))
        {
            transform.position = Vector3.MoveTowards(transform.position, originPos, moveSpeed * Time.deltaTime);
            distacne = Vector3.Distance(originPos, transform.position);

            yield return null;
        }

        StartCoroutine(moveToShow());
    }
}
