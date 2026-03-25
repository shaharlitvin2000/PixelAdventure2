using System.Collections;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;

    [Header("Unique Settings")]
    public string voiceSoundName = "NPC";

    private int dialogueIndex;
    private bool isTyping, isDialogueActive, isWaitingForChoice;

    void Start()
    {
        dialogueUI = DialogueController.Instance;
        if (dialogueUI == null)
            Debug.LogError("DialogueController not found in scene!");
    }

    public bool CanInteract() => !isDialogueActive;

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;

        if (isDialogueActive)
            NextLine();
        else
            StartDialogue();
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        PauseController.SetPause(true);
        StartCoroutine(TypeLine());
    }

    void Update()
    {
        if (isDialogueActive && dialogueUI != null && !dialogueUI.dialoguePanel.activeInHierarchy)
        {
            EndDialogue();
            return;
        }

        // לא מאפשר קליק בזמן בחירה
        if (isWaitingForChoice) return;

        if (isDialogueActive && Input.GetMouseButtonDown(0))
            NextLine();
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;

            // בדוק endDialogue אחרי Skip
            if (dialogueData.endDialogueLines.Length > dialogueIndex &&
                dialogueData.endDialogueLines[dialogueIndex])
            {
                EndDialogue();
                return;
            }

            CheckForChoices();
            return;
        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.dialogueText.text += letter;
            if (letter != ' ' && SoundEffectManager.Instance != null)
                SoundEffectManager.Instance.Play(voiceSoundName);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        // בדוק אם השורה הזו מסיימת את הדיאלוג
        if (dialogueData.endDialogueLines.Length > dialogueIndex &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            EndDialogue();
            yield break;
        }

        // Auto progress
        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
            yield break;
        }

        // בדוק בחירות
        CheckForChoices();
    }

    void CheckForChoices()
    {
        if (dialogueData.choices == null) return;

        foreach (DialogueChoice choice in dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                ShowChoices(choice);
                return;
            }
        }
    }

    void ShowChoices(DialogueChoice choice)
    {
        isWaitingForChoice = true;

        dialogueUI.ShowChoices(choice.choices, (selectedIndex) =>
        {
            isWaitingForChoice = false;
            int nextIndex = choice.nextDialogueIndex[selectedIndex];

            // -1 = סיום דיאלוג
            if (nextIndex < 0)
            {
                EndDialogue();
                return;
            }

            dialogueIndex = nextIndex;
            StartCoroutine(TypeLine());
        });
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        isWaitingForChoice = false;
        dialogueUI.HideChoices();

        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText("");
            dialogueUI.ShowDialogueUI(false);
        }

        PauseController.SetPause(false);
    }
}