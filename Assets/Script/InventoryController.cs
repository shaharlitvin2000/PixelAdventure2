using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;
    Dictionary<int, int> itemCountCache = new();
    public event Action OnInventoryChange;

    [Header("References")]
    [SerializeField] private ItemDictionary itemDictionary;
    [SerializeField] public GameObject inventoryPanel;
    [SerializeField] private GameObject slotPrefab;

    [Header("Settings")]
    [SerializeField] private int slotCount = 20;

    private void Awake()
    {
        instance = this;

        if (itemDictionary == null)
            itemDictionary = FindObjectOfType<ItemDictionary>();

        if (itemDictionary == null)
            Debug.LogError("ItemDictionary not found in scene!");

        if (inventoryPanel == null)
            Debug.LogError("inventoryPanel is NULL!");

        if (slotPrefab == null)
            Debug.LogError("slotPrefab is NULL!");

        EnsureSlotCount();
        RebuildItemCounts(); // ✅ אחרי אתחול
    }

    public void RebuildItemCounts()
    {
        itemCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemCountCache[item.ID] = itemCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        OnInventoryChange?.Invoke(); // FIX: null-safe — קרס אם אין מאזינים
    }

    public Dictionary<int, int> GetItemCounts() => itemCountCache;

    public bool AddItem(GameObject itemPrefab, int quantity = 1)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack(quantity);
                    RebuildItemCounts(); // ✅ אחרי stack
                    return true;
                }
            }
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                RectTransform rect = newItem.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one;

                Item newItemComponent = newItem.GetComponent<Item>();
                if (newItemComponent != null)
                {
                    newItemComponent.quantity = quantity;
                    newItemComponent.UpdateQuantityDisplay();
                }

                slot.currentItem = newItem;
                RebuildItemCounts(); // ✅ אחרי הוספה לסלוט חדש
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }

    private void EnsureSlotCount()
    {
        while (inventoryPanel.transform.childCount < slotCount)
            Instantiate(slotPrefab, inventoryPanel.transform);

        while (inventoryPanel.transform.childCount > slotCount)
            DestroyImmediate(
                inventoryPanel.transform
                .GetChild(inventoryPanel.transform.childCount - 1)
                .gameObject
            );
    }

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
                // FIX: הוצא את RebuildItemCounts מתוך ה-initializer — syntax error
                saveData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = i,
                    quantity = item.quantity,
                });
            }
        }

        RebuildItemCounts(); // ✅ אחרי קריאת כל הפריטים
        return saveData;
    }

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

            Item itemComponent = itemInstance.GetComponent<Item>();
            if (itemComponent != null)
            {
                itemComponent.quantity = data.quantity;
                itemComponent.UpdateQuantityDisplay();
            }

            slot.currentItem = itemInstance;
        }

        RebuildItemCounts(); // ✅ אחרי טעינת כל הפריטים מהסייב
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

        RebuildItemCounts(); // ✅ אחרי ניקוי כל הסלוטים
    }
}