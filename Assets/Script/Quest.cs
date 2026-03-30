using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;
    public List<QuestObjectives> onjectives;

    [Header("Quest Rewards")]
    public QuestReward[] questRewards; // FIX: נוסף שדה פרסים

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = questName + Guid.NewGuid().ToString();
        }
    }
}

// FIX: נוסף קלאס פרס
[System.Serializable]
public class QuestReward
{
    public RewardType type;
    public int rewardID;  // עבור Item - זה ה-itemID
    public int amount;
}

// FIX: נוסף enum לסוגי פרסים
public enum RewardType
{
    Item,
    Gold,
    Expirance,
    Weapon,
    Custom
}

[System.Serializable]
public class QuestObjectives
{
    public string objectiveID;
    public string description;
    public objectiveType type;
    public int requiredAmount;
    public int currentAmount;
    public bool IsCompleted => currentAmount >= requiredAmount;
}

public enum objectiveType { CollectItem, DefetEnemy, ReachLocation, TalkNPC, Custom }

[System.Serializable]
public class QuestPrograss
{
    public Quest quest;
    public List<QuestObjectives> objectives;

    public QuestPrograss(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjectives>();
        foreach (var obj in quest.onjectives)
        {
            objectives.Add(new QuestObjectives
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);
    public string QuestID => quest.questID;
}