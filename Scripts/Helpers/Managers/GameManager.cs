using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    private bool canRespone = false;

    [SerializeField]
    private Image sceneFader;

    private Color sceneFaderOriginColor;

    private bool isGetCenterHallKey;

    private const float fadeInSpeed = 1.0f;
    private const float fadeOutSpeed = 1.5f;
    private const float fadeDelay = 0.0f;



    private void Awake()
    {
        Option.instance.newScene();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isGetCenterHallKey = false;

        sceneFaderOriginColor = sceneFader.color;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canRespone)
        {
            loadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void setCanRespone(bool val)
    {
        canRespone = val;
    }

    public void loadScene(string gameStr)
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(gameStr);
    }

    public void loadSceneWithFadeIn(string gameStr)
    {
        StartCoroutine(loadSceneWithFadeInCoroutine(gameStr));
    }


    IEnumerator loadSceneWithFadeInCoroutine(string gameStr)
    {
        sceneFader.enabled = true;

        while (Mathf.Abs(sceneFader.color.a - sceneFaderOriginColor.a) > 0.01f)
        {
            sceneFader.color = Color.Lerp(sceneFader.color,
                sceneFaderOriginColor, fadeInSpeed * Time.deltaTime);

            yield return null;
        }

        sceneFader.color = sceneFaderOriginColor;

        Time.timeScale = 1;
        SceneManager.LoadScene(gameStr);
    }

    public void fadeOutScene()
    {
        StartCoroutine(fadeOutSceneCoroutine());
    }

    IEnumerator fadeOutSceneCoroutine()
    {
        sceneFader.enabled = true;

        yield return new WaitForSeconds(fadeDelay);

        while (sceneFader.color.a > 0.2f)
        {
            Color color = new Color(sceneFader.color.r, sceneFader.color.g, sceneFader.color.b, 0);

            sceneFader.color = Color.Lerp(sceneFader.color, color, fadeOutSpeed * Time.deltaTime);

            yield return null;
        }

        sceneFader.enabled = false;
    }


    public void exitGame()
    {
        Application.Quit();
    }

    public void pickUpCenterHallKey()
    {
        isGetCenterHallKey = true;
    }
}
