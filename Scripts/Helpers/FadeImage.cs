using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeImage : MonoBehaviour
{
    [SerializeField]
    private float fadeOutDelay = 2.0f;

    [SerializeField]
    private float fadeInSpeed = 1.0f;

    [SerializeField]
    private float fadeOutSpeed = 1.0f;

    [SerializeField]
    private FadeText text;

    private Color originColor;
    private Image image;

    void Start()
    {
        image = GetComponent<Image>();

        originColor = image.color;

    }

    public void fadeInCoroutine(string str)
    {
        StartCoroutine(fadeIn(str));
    }

    private IEnumerator fadeIn(string str)
    {
        str = str.Replace("\\n", "\n");
        text.fadeInCoroutine(str);

        this.gameObject.GetComponent<Image>().enabled = true;

        while (Mathf.Abs(image.color.a - originColor.a) > 0.01f)
        {
            image.color = Color.Lerp(image.color, originColor, fadeInSpeed * Time.deltaTime);

            yield return null;
        }
    }

    public void fadeInAndOutCoroutine(string str)
    {
        StartCoroutine(fadeInOut(str));
    }


    private IEnumerator fadeInOut(string str)
    {
        str = str.Replace("\\n", "\n");
        text.fadeInOutCoroutine(str);

        this.gameObject.GetComponent<Image>().enabled = true;

        while (Mathf.Abs(image.color.a - originColor.a) > 0.01f)
        {
            image.color = Color.Lerp(image.color, originColor, fadeInSpeed * Time.deltaTime);

            yield return null;
        }

        StartCoroutine(fadeOut());
    }

    public void fadeOutCoroutine()
    {
        StartCoroutine(fadeOut());
    }

    private IEnumerator fadeOut()
    {
        yield return new WaitForSeconds(fadeOutDelay);

        while (image.color.a > 0.2f)
        {
            Color color = new Color(image.color.r, image.color.g, image.color.b, 0);

            image.color = Color.Lerp(image.color, color, fadeOutSpeed * Time.deltaTime);

            yield return null;
        }

        this.gameObject.GetComponent<Image>().enabled = false;
    }
}
