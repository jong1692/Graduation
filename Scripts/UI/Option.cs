using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Option : MonoBehaviour
{
    public static Option instance;

    private bool isPaused;
    private bool isMute;

    [SerializeField]
    private GameObject option;

    [SerializeField]
    private Toggle muteToggle;

    [SerializeField]
    private Slider bgmVolSlider;

    [SerializeField]
    private GameObject audioOption;

    private AudioListener audioListener;
    private AudioSource bgm;


    private float volume = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);

            isPaused = false;
            isMute = false;

            Option.instance.newScene();
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
        {
            showOption();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused)
        {
            hideOption();
        }
    }

    public void newScene()
    {
        bgm = GameObject.Find("CameraBrain").GetComponent<AudioSource>();
        bgm.volume = volume;

        audioListener = GameObject.Find("CameraBrain").GetComponent<AudioListener>();
        audioListener.enabled = !isMute;

        if (SceneManager.GetActiveScene().name == "Title")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }


    public void setAudioOption()
    {
        audioOption.SetActive(!audioOption.activeInHierarchy);
    }


    public void showOption()
    {
        isPaused = true;
        muteToggle.isOn = isMute;
        Time.timeScale = 0;

        bgm = GameObject.Find("CameraBrain").GetComponent<AudioSource>();

        bgmVolSlider.value = volume;

        option.gameObject.SetActive(true);



        if (SceneManager.GetActiveScene().name != "Title")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void setBgmVol()
    {
        volume = bgmVolSlider.value;
        bgm.volume = volume;
    }




    public void hideOption()
    {
        isPaused = false;

        Time.timeScale = 1;


        if (SceneManager.GetActiveScene().name != "Title")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        option.gameObject.SetActive(false);
    }

    public void setMuteSound()
    {
        isMute = muteToggle.isOn;
        audioListener.enabled = !isMute;

        if (isMute)
            AudioListener.volume = 0;
        else
            AudioListener.volume = 1;
    }

    public void exitGame()
    {
        Application.Quit();
    }


    public void loadScene(string gameStr)
    {
        Time.timeScale = 1;

        SceneManager.LoadScene(gameStr);
    }
}
