using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DialogueLoader : MonoBehaviour
{
    [System.Serializable]
    public struct DialogueInfo
    {
        public string id;
        public float changeNextTextDelay;
        public bool useAutoDestroy;
        public bool canDetectInput;

        public UnityEvent startEvent;
        public UnityEvent endEvent;

        public System.Action startAction;
        public System.Action endAction;
    }

    [SerializeField]
    private List<DialogueInfo> dialogueInfoList = new List<DialogueInfo>();

    PlayerInput playerInput;

    private int dialogueInfoCnt;

    private bool active;

    private void Start()
    {
        playerInput = PlayerInput.Instance;

        active = false;

        dialogueInfoCnt = 0;
    }

    private void Update()
    {
        if (!active) return;

        //if (playerInput.getKeyDown(KeyCode.E) && !useAutoDestroy)
        //    hideDialogue();
    }

    public void loadAndShowDialogue()
    {
        if (this.enabled == false) return;

        DialogueManager.Instance.loadDialogueList(dialogueInfoList);

        Destroy(this.gameObject);
    }

}
