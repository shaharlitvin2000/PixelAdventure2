using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController instance { get; private set; }
    public List<QuestPrograss> activateQuest = new();
    public List<QuestPrograss> completedQuests = new();
    public List<string> handinQuestIDs = new();
    private QuestUI questUI;

    [Header("All Quests In Game")]
    public List<Quest> allQuests;

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

    public bool IsQuestHandedIn(string questID) =>
        handinQuestIDs.Contains(questID);

    public bool IsQuestObjectivesComplete(string questID)
    {
        QuestPrograss quest = activateQuest.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

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
        }

        questUI?.UpdateQuestUI();
    }

    public void HandInQuest(string questID)
    {
        if (!RemoveRequiredItemsFromInventory(questID))
            return;

        QuestPrograss quest = activateQuest.Find(q => q.QuestID == questID);
        if (quest != null)
        {
            handinQuestIDs.Add(quest.QuestID);
            activateQuest.Remove(quest);
            questUI?.UpdateQuestUI();
            Debug.Log($"Quest {questID} handed in successfully!");
        }
    }

    public bool RemoveRequiredItemsFromInventory(string questID)
    {
        QuestPrograss quest = activateQuest.Find(q => q.QuestID == questID);
        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();

        foreach (QuestObjectives objective in quest.objectives)
        {
            if (objective.type == objectiveType.CollectItem &&
                int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objective.requiredAmount;
            }
        }

        Dictionary<int, int> itemCounts = GetCombinedItemCounts();
        foreach (var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key) < item.Value)
            {
                Debug.Log("Not enough items to hand in quest!");
                return false;
            }
        }

        foreach (var itemRequired in requiredItems)
        {
            int remaining = itemRequired.Value;

            remaining = InventoryController.instance.RemoveItemsAndReturnRemaining(itemRequired.Key, remaining);

            if (remaining > 0)
            {
                HotbarController hotbar = FindObjectOfType<HotbarController>();
                if (hotbar != null)
                    hotbar.RemoveItemFromHotbar(itemRequired.Key, remaining);
            }
        }

        return true;
    }

    public List<QuestSaveData> GetQuestSaveData()
    {
        List<QuestSaveData> saveData = new();
        foreach (QuestPrograss quest in activateQuest)
        {
            QuestSaveData qsd = new QuestSaveData
            {
                questID = quest.QuestID,
                objectives = new List<ObjectiveSaveData>()
            };
            foreach (QuestObjectives obj in quest.objectives)
            {
                qsd.objectives.Add(new ObjectiveSaveData
                {
                    objectiveID = obj.objectiveID,
                    currentAmount = obj.currentAmount
                });
            }
            saveData.Add(qsd);
        }
        return saveData;
    }

    public void LoadQuestProgress(List<QuestSaveData> savedQuests)
    {
        if (savedQuests == null) return;

        foreach (QuestSaveData savedQuest in savedQuests)
        {
            Quest questAsset = allQuests.Find(q => q.questID == savedQuest.questID);
            if (questAsset == null)
            {
                Debug.LogWarning($"Quest asset not found for ID: {savedQuest.questID}");
                continue;
            }

            if (IsQuestActive(savedQuest.questID) || IsQuestCompleted(savedQuest.questID))
                continue;

            QuestPrograss progress = new QuestPrograss(questAsset);

            foreach (QuestObjectives obj in progress.objectives)
            {
                ObjectiveSaveData saved = savedQuest.objectives?.Find(o => o.objectiveID == obj.objectiveID);
                if (saved != null)
                    obj.currentAmount = saved.currentAmount;
            }

            activateQuest.Add(progress);
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