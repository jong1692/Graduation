using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject show;
    [SerializeField]
    private GameObject hide;

    private void Start()
    {
        Option.instance.newScene();
    }

    void Update()
    {
        if (Input.anyKeyDown && show.activeInHierarchy == false)
        {
            show.SetActive(true);
            hide.SetActive(false);
        }
    }

    public void loadScene(string gameStr)
    {
        SceneManager.LoadScene(gameStr);
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void loadYoutube()
    {
        Application.OpenURL(" https://www.youtube.com/watch?v=jcYIowquDJs");
    }
}
