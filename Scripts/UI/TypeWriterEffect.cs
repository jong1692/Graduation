using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TypeWriterEffect : MonoBehaviour
{
    [SerializeField]
    private float typeWriteDelay = 0.2f;
    private float typeWriteTimer;

    [SerializeField]
    private Text dialogueText;
    private string dialogue;
    private int dialogueCnt;
    private float changeNextTextDelay;

    private DialogueManager dialogueManager;

    void Awake()
    {
        typeWriteTimer = 0;
    }

    private void Start()
    {
        dialogueManager = DialogueManager.Instance;
    }
    void Update()
    {
       
    }

    public void setDialogue(string dialogue, float changeNextTextDelay)
    {
        this.dialogue = dialogue;
        this.changeNextTextDelay = changeNextTextDelay;
    }

    public IEnumerator startTypeWrite()
    {
        dialogueText.text = "";
        dialogueCnt = 0;
        typeWriteTimer = typeWriteDelay;
        
        while (dialogueCnt != dialogue.Length)
        {
            typeWriteTimer += Time.deltaTime;

            if (typeWriteDelay < typeWriteTimer)
            {
                typeWriteTimer = 0;
                dialogueText.text += dialogue[dialogueCnt++];
            }

            yield return null;
        }

        yield return new WaitForSeconds(changeNextTextDelay);

        dialogueManager.showNextDialogue();
    }
}
