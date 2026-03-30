using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardController : MonoBehaviour
{
    public static RewardController instance { get; private set; }

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest?.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Gold:
                    // TODO: הוסף מערכת זהב
                    break;
                case RewardType.Expirance:
                    // TODO: הוסף מערכת ניסיון
                    break;
                case RewardType.Weapon:
                    // TODO: הוסף מערכת נשק
                    break;
                case RewardType.Custom:
                    // TODO: הוסף פרסים מיוחדים
                    break;
            }
        }
    }

    public void GiveItemReward(int itemID, int amount)
    {
        Debug.Log($"Trying to give item {itemID} x{amount}");

        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);
        if (itemPrefab == null)
        {
            Debug.LogWarning($"Item with ID {itemID} not found!");
            return;
        }

        // FIX: הוסף פריט אחד בכל פעם כדי שכל אחד יציג popup נפרד
        int itemsAdded = 0;

        for (int i = 0; i < amount; i++)
        {
            if (InventoryController.instance.AddItem(itemPrefab, 1)) // FIX: אחד בכל פעם
            {
                itemsAdded++;

                // FIX: popup עם עיכוב קטן כדי שלא יהיו אחד על השני
                StartCoroutine(ShowPopupWithDelay(itemPrefab, i * 0.1f));
            }
            else
            {
                // אינוונטורי מלא - זרוק על הרצפה
                Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
                Vector3 dropPosition = player != null ? player.position : transform.position;

                // FIX: מפזר את הפריטים במקום לזרוק אחד על השני
                Vector3 randomOffset = new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    0,
                    Random.Range(-0.5f, 0.5f)
                );

                GameObject dropItem = Instantiate(itemPrefab, dropPosition + Vector3.down + randomOffset, Quaternion.identity);
                dropItem.GetComponent<BounceEffect>()?.StartBounce();
            }
        }

        Debug.Log($"Added {itemsAdded} items to inventory, dropped {amount - itemsAdded} items");
    }

    // FIX: פונקציה חדשה להצגת popup עם עיכוב
    private IEnumerator ShowPopupWithDelay(GameObject itemPrefab, float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject tempItem = Instantiate(itemPrefab);
        tempItem.GetComponent<Item>()?.ShowPopUp();
        Destroy(tempItem);
    }
}