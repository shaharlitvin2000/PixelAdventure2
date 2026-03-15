using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    [Header("Unique Settings")]
    [Tooltip("השם המדויק של קבוצת הסאונד ב-SoundEffectLibrary")]
    public string voiceSoundName = "NPC";

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
        {
            return;
        }
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void Update()
    {
        // פותר את בעיית ה-X: אם הפאנל נסגר ידנית, הקוד מתאפס
        if (isDialogueActive && dialoguePanel != null && !dialoguePanel.activeInHierarchy)
        {
            EndDialogue();
            return;
        }

        // זירוז טקסט
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if (++dialogueIndex < dialogueData.dialogueLines.Length)
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
        dialogueText.SetText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;

            if (letter != ' ' && SoundEffectManager.Instance != null)
            {
                // משתמש במשתנה voiceSoundName במקום ב-"NPC" קבוע
                SoundEffectManager.Instance.Play(voiceSoundName);
            }

            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        if (dialogueText != null) dialogueText.SetText("");
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
    }
}