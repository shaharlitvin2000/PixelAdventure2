using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController instance { get; private set; }
    public List<QuestPrograss> activateQuest = new();
    public List<QuestPrograss> completedQuests = new(); // FIX: track completed quests
    private QuestUI questUI;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        questUI = FindObjectOfType<QuestUI>();
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID) || IsQuestCompleted(quest.questID)) return;
        activateQuest.Add(new QuestPrograss(quest));
        questUI?.UpdateQuestUI(); // FIX: null-safe in case QuestUI is missing
    }

    // FIX: added CompleteQuest so quests can be moved to completedQuests
    public void CompleteQuest(string questID)
    {
        QuestPrograss q = activateQuest.Find(q => q.QuestID == questID);
        if (q == null) return;
        activateQuest.Remove(q);
        completedQuests.Add(q);
        questUI?.UpdateQuestUI();
    }

    public bool IsQuestActive(string questID) =>
        activateQuest.Exists(q => q.QuestID == questID);

    // FIX: added IsQuestCompleted so NPC.cs SyncQuestState works
    public bool IsQuestCompleted(string questID) =>
        completedQuests.Exists(q => q.QuestID == questID);
}