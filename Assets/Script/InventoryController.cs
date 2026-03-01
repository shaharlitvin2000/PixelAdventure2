using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemDictionary itemDictionary;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject slotPrefab;

    [Header("Settings")]
    [SerializeField] private int slotCount = 20;


    public bool AddItem(GameObject itemPrefab)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);

                RectTransform rect = newItem.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;

                slot.currentItem = newItem;

                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }
    private void Awake()
    {
        if (itemDictionary == null)
            itemDictionary = FindObjectOfType<ItemDictionary>();

        if (itemDictionary == null)
            Debug.LogError("ItemDictionary not found in scene!");

        if (inventoryPanel == null)
            Debug.LogError("inventoryPanel is NULL!");

        if (slotPrefab == null)
            Debug.LogError("slotPrefab is NULL!");

        EnsureSlotCount();
    }

    private void EnsureSlotCount()
    {
        // אם יש פחות מדי סלוטים — תיצור
        while (inventoryPanel.transform.childCount < slotCount)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // אם יש יותר מדי — תמחק
        while (inventoryPanel.transform.childCount > slotCount)
        {
            DestroyImmediate(
                inventoryPanel.transform
                .GetChild(inventoryPanel.transform.childCount - 1)
                .gameObject
            );
        }
    }

    // ================================
    // SAVE
    // ================================
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> saveData = new List<InventorySaveData>();

        for (int i = 0; i < inventoryPanel.transform.childCount; i++)
        {
            Transform slotTransform = inventoryPanel.transform.GetChild(i);
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                saveData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = i
                });
            }
        }

        return saveData;
    }

    // ================================
    // LOAD
    // ================================
    public void SetInventoryItems(List<InventorySaveData> savedItems)
    {
        if (itemDictionary == null)
        {
            Debug.LogError("ItemDictionary is NULL!");
            return;
        }

        EnsureSlotCount();
        ClearAllSlots();

        foreach (InventorySaveData data in savedItems)
        {
            if (data.slotIndex < 0 || data.slotIndex >= slotCount)
                continue;

            GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

            if (itemPrefab == null)
            {
                Debug.LogWarning("ItemPrefab not found for ID: " + data.itemID);
                continue;
            }

            Transform slotTransform = inventoryPanel.transform.GetChild(data.slotIndex);
            Slot slot = slotTransform.GetComponent<Slot>();

            GameObject itemInstance = Instantiate(itemPrefab, slotTransform);
            itemInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            slot.currentItem = itemInstance;
        }
    }

    private void ClearAllSlots()
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Slot slot = child.GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
        }
    }
}