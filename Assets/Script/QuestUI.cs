using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContant;
    public GameObject questEnteryPrefab;
    public GameObject objectiveTextPrefab;

    // FIX: removed testQuest/testQuestAmount — testing via Inspector caused
    // duplicate fake quests to appear alongside real ones. Use QuestController directly.

    void Start()
    {
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        foreach (Transform child in questListContant)
            Destroy(child.gameObject);

        // FIX: null guard in case QuestController isn't in scene yet
        if (QuestController.instance == null)
        {
            Debug.LogWarning("QuestUI: QuestController instance not found.");
            return;
        }

        foreach (var quest in QuestController.instance.activateQuest)
        {
            GameObject entry = Instantiate(questEnteryPrefab, questListContant);

            TMP_Text questNameText = entry.transform.Find("QuestNameText")?.GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            // FIX: null guards so missing prefab children don't crash the game
            if (questNameText == null)
            {
                Debug.LogError("QuestUI: 'QuestNameText' not found in questEntryPrefab.");
                continue;
            }

            if (objectiveList == null)
            {
                Debug.LogError("QuestUI: 'ObjectiveList' not found in questEntryPrefab.");
                continue;
            }

            questNameText.text = quest.quest.questName; // FIX: use questName field, not .name (that's the asset name)

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGo = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGo.GetComponent<TMP_Text>();

                if (objText == null)
                {
                    Debug.LogError("QuestUI: objectiveTextPrefab has no TMP_Text component.");
                    continue;
                }

                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}