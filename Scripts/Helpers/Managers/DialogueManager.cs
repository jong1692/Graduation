using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;

    public static DialogueManager Instance
    {
        get { return instance; }
    }

    [System.Serializable]
    public struct Dialogue
    {
        public string id;
        public string speaker;
        public string main;
        public string sub;
        public string text;
    }

    private class Wrapper
    {
        public Dialogue[] items;
    }

    [SerializeField]
    private GameObject diaogueBox;
    [SerializeField]
    private Text speakerNameText;
    [SerializeField]
    private Text mainText;
    [SerializeField]
    private Text subText;
    [SerializeField]
    private Text diaogueText;
    [SerializeField]
    private GameObject hpBars;
    [SerializeField]
    private UnityEngine.UI.Image dialogueBox;
    [SerializeField]
    private TextBoxController missionBox;

    private PlayerInput playerInput;
    private TimeManager timeManager;

    private List<DialogueLoader.DialogueInfo> curDialogueInfoList;
    private List<Dialogue> curDialogueList;

    private int dialogueCnt;

    [SerializeField]
    private UnityEvent startEvent;
    private UnityAction startAction;

    void Awake()
    {
        initialize();
    }

    void Start()
    {
        playerInput = PlayerInput.Instance;

        timeManager = TimeManager.Instance;

        startAction += startEvent.Invoke;
        startAction();
    }

    private void initialize()
    {
        if (instance == null)
        {
            instance = this;
        }

        dialogueCnt = 0;

        curDialogueInfoList = new List<DialogueLoader.DialogueInfo>();
        curDialogueList = new List<Dialogue>();
    }

    private void showDialogue()
    {
        if (curDialogueInfoList[dialogueCnt].canDetectInput == false)
            playerInput.CanDetectInput = false;

        speakerNameText.text = curDialogueList[dialogueCnt].speaker;
        mainText.text = curDialogueList[dialogueCnt].main;
        subText.text = curDialogueList[dialogueCnt].sub;

        TypeWriterEffect typeWriterEffect = diaogueText.GetComponent<TypeWriterEffect>();
        if (typeWriterEffect != null)
        {
            typeWriterEffect.setDialogue(curDialogueList[dialogueCnt].text, curDialogueInfoList[dialogueCnt].changeNextTextDelay);
            StartCoroutine(typeWriterEffect.startTypeWrite());
        }

        var startAction = curDialogueInfoList[dialogueCnt].startAction;
        startAction += curDialogueInfoList[dialogueCnt].startEvent.Invoke;

        startAction();
    }

    public void hideMission()
    {
        missionBox.hide();
    }

    public void showNextMission()
    {
        missionBox.show();
    }

    public void hideAndshowNextMission()
    {
        missionBox.hideAndShow();
    }

    public void showNextDialogue()
    {
        var endAction = curDialogueInfoList[dialogueCnt].endAction;
        endAction += curDialogueInfoList[dialogueCnt].endEvent.Invoke;

        endAction();

        if (curDialogueInfoList.Count != ++dialogueCnt)
            showDialogue();
        else
        {
            if (hpBars != null)
                hpBars.SetActive(true);

            if (playerInput != null)
            {
                if (playerInput.CanDetectInput == false)
                    playerInput.CanDetectInput = true;
            }

            if (curDialogueInfoList[dialogueCnt - 1].useAutoDestroy)
                hideDialgueBox();
        }
    }

    public void hideDialgueBox()
    {
        diaogueBox.SetActive(false);
    }

    public void loadDialogueList(List<DialogueLoader.DialogueInfo> dialogueInfoList)
    {
        //string jsonData = File.ReadAllText(Application.dataPath + "/Resources/Dialogue/Dialogue.json");

        var textAsset = Resources.Load<TextAsset>("Dialogue/Dialogue");
        StringReader textReader = new StringReader(textAsset.text);

        Wrapper wrapper = new Wrapper();
        wrapper = JsonUtility.FromJson<Wrapper>(textAsset.text);

        List<Dialogue> dialogueList = new List<Dialogue>();
        for (int i = 0; i < dialogueInfoList.Count; i++)
        {
            foreach (var dialogue in wrapper.items)
            {
                //if (dialogue.id == "None") return;

                if (dialogue.id == dialogueInfoList[i].id)
                {
                    dialogueList.Add(dialogue);
                    break;
                }
            }
        }

        curDialogueList = dialogueList;
        curDialogueInfoList = dialogueInfoList;

        if (diaogueBox.activeInHierarchy == false)
            diaogueBox.SetActive(true);

        if (hpBars.activeInHierarchy == true)
            hpBars.SetActive(false);

        dialogueCnt = 0;

        showDialogue();
    }
}
