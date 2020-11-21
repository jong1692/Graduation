using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeText : MonoBehaviour
{
    [SerializeField]
    private float fadeOutDelay = 2.0f;

    [SerializeField]
    private float fadeInSpeed = 1.0f;

    [SerializeField]
    private float fadeOutSpeed = 1.0f;

    private Color originColor;
    private Text text;

    void Start()
    {
        text = GetComponent<Text>();

        originColor = text.color;

        Invoke("fadeOutCoroutine", fadeOutDelay);
    }

    public void fadeInOutCoroutine(string str)
    {
        StartCoroutine(fadeInOut(str));
    }

    private IEnumerator fadeInOut(string str)
    {
        text.text = str;
        text.enabled = true;

        while (Mathf.Abs(text.color.a - originColor.a) > 0.01f)
        {
            text.color = Color.LerpUnclamped(text.color, originColor, fadeInSpeed * Time.deltaTime);

            yield return null;
        }

        Invoke("fadeOutCoroutine", fadeOutDelay);

        yield return null;
    }

    public void fadeInCoroutine(string str)
    {
        StartCoroutine(fadeIn(str));
    }

    private IEnumerator fadeIn(string str)
    {
        text.text = str;
        text.enabled = true;

        while (Mathf.Abs(text.color.a - originColor.a) > 0.01f)
        {
            text.color = Color.Lerp(text.color, originColor, fadeInSpeed * Time.deltaTime);

            yield return null;
        }
    }

    public void fadeOutCoroutine()
    {
        StartCoroutine(fadeOut());
    }

    private IEnumerator fadeOut()
    {
        while (text.color.a > 0.2f)
        {
            Color color = new Color(text.color.r, text.color.g, text.color.b, 0);

            text.color = Color.Lerp(text.color, color, fadeOutSpeed * Time.deltaTime);

            yield return null;
        }

        text.enabled = false;
    }
}
