using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController instance { get; private set; }
    public List<QuestPrograss> activateQuest = new();
    public List<QuestPrograss> completedQuests = new();
    private QuestUI questUI;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        questUI = FindObjectOfType<QuestUI>();
        InventoryController.instance.OnInventoryChange += CheackInventoryForQuests;
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID) || IsQuestCompleted(quest.questID)) return;
        activateQuest.Add(new QuestPrograss(quest));
        CheackInventoryForQuests();
        questUI?.UpdateQuestUI();
    }

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

    public bool IsQuestCompleted(string questID) =>
        completedQuests.Exists(q => q.QuestID == questID);

    public void CheackInventoryForQuests()
    {
        Dictionary<int, int> itemCounts = GetCombinedItemCounts();

        foreach (QuestPrograss quest in activateQuest)
        {
            foreach (QuestObjectives questObjectives in quest.objectives)
            {
                if (questObjectives.type != objectiveType.CollectItem) continue;
                if (!int.TryParse(questObjectives.objectiveID, out int itemID)) continue;

                int newAmount = itemCounts.TryGetValue(itemID, out int count)
                    ? Mathf.Min(count, questObjectives.requiredAmount)
                    : 0;

                if (questObjectives.currentAmount != newAmount)
                    questObjectives.currentAmount = newAmount;
            }
            // הוסר: בדיקת השלמה אוטומטית
        }

        questUI?.UpdateQuestUI();
    }

    private Dictionary<int, int> GetCombinedItemCounts()
    {
        Dictionary<int, int> combined = new();

        foreach (var pair in InventoryController.instance.GetItemCounts())
            combined[pair.Key] = combined.GetValueOrDefault(pair.Key, 0) + pair.Value;

        HotbarController hotbar = FindObjectOfType<HotbarController>();
        if (hotbar != null)
        {
            foreach (Transform slotTransform in hotbar.hotbarPanel.transform)
            {
                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot?.currentItem != null)
                {
                    Item item = slot.currentItem.GetComponent<Item>();
                    if (item != null)
                        combined[item.ID] = combined.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        return combined;
    }
}