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
    private bool inputCooldown;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState queststate = QuestState.NotStarted;

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
        SyncQuestState();

        if (queststate == QuestState.NotStarted)
            dialogueIndex = 0;
        else if (queststate == QuestState.InProgress)
            dialogueIndex = dialogueData.questInProgressIndex;
        else if (queststate == QuestState.Completed)
            dialogueIndex = dialogueData.questCompletedIndex;

        isDialogueActive = true;
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        PauseController.SetPause(true);
        StartCoroutine(TypeLine());
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        // בדיקת סדר נכון: HandedIn > ObjectivesComplete > Active > NotStarted
        if (QuestController.instance.IsQuestHandedIn(questID))
        {
            queststate = QuestState.Completed;
        }
        else if (QuestController.instance.IsQuestActive(questID) &&
                 QuestController.instance.IsQuestObjectivesComplete(questID))
        {
            queststate = QuestState.Completed; // אובייקטיבים הושלמו - הצג הודיה
        }
        else if (QuestController.instance.IsQuestActive(questID))
        {
            queststate = QuestState.InProgress;
        }
        else
        {
            queststate = QuestState.NotStarted;
        }
    }

    void Update()
    {
        if (isDialogueActive && dialogueUI != null && !dialogueUI.dialoguePanel.activeInHierarchy)
        {
            EndDialogue();
            return;
        }

        if (isWaitingForChoice) return;
        if (inputCooldown) return;

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

            if (dialogueData.endDialogueLines.Length > dialogueIndex &&
                dialogueData.endDialogueLines[dialogueIndex])
            {
                EndDialogue();
                return;
            }

            StartCoroutine(InputCooldown());
            CheckForChoices();
            return;
        }

        if (dialogueData.endDialogueLines.Length > dialogueIndex &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
            StartCoroutine(TypeLine());
        else
            EndDialogue();
    }
    IEnumerator TypeLine()
    {
        isTyping = true;
        inputCooldown = true;
        dialogueUI.SetDialogueText("");

        // תיקון: המתן שני פריימים במקום אחד
        yield return null;
        yield return null;
        inputCooldown = false;

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.dialogueText.text += letter;
            if (letter != ' ' && SoundEffectManager.Instance != null)
                SoundEffectManager.Instance.Play(voiceSoundName);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.endDialogueLines.Length > dialogueIndex &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            if (dialogueData.autoProgressDelay > 0)
                yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            EndDialogue();
            yield break;
        }

        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
            yield break;
        }

        CheckForChoices();
    }

    IEnumerator InputCooldown()
    {
        inputCooldown = true;
        // תיקון: המתן שני פריימים במקום אחד
        yield return null;
        yield return null;
        inputCooldown = false;
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

            if (choice.givesQuest != null &&
                selectedIndex < choice.givesQuest.Length &&
                choice.givesQuest[selectedIndex] &&
                dialogueData.quest != null)
            {
                QuestController.instance.AcceptQuest(dialogueData.quest);
                SyncQuestState();
            }

            int nextIndex = choice.nextDialogueIndex[selectedIndex];

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
        // אם הדיאלוג נגמר והמשימה מושלמת - תן פרסים והחזר פריטים
        if (dialogueData.quest != null &&
            queststate == QuestState.Completed &&
            QuestController.instance.IsQuestActive(dialogueData.quest.questID) &&
            QuestController.instance.IsQuestObjectivesComplete(dialogueData.quest.questID))
        {
            HandleQuestCompletion(dialogueData.quest); // FIX: תוקן שם הפונקציה וקריאה
        }

        StopAllCoroutines();
        isDialogueActive = false;
        isWaitingForChoice = false;
        inputCooldown = false;
        dialogueUI.HideChoices();

        if (dialogueUI != null)
        {
            dialogueUI.SetDialogueText("");
            dialogueUI.ShowDialogueUI(false);
        }

        PauseController.SetPause(false);
    }

    void HandleQuestCompletion(Quest quest) // FIX: תוקן שם הפונקציה
    {
        RewardController.instance.GiveQuestReward(quest);
        QuestController.instance.HandInQuest(quest.questID);
    }
}