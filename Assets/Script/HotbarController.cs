using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 7;

    private ItemDictionary itemDictionary;

    private Key[] hotbarKeys;

    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            hotbarKeys[i] = i < 6 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }

        if (itemDictionary == null)
            Debug.LogError("ItemDictionary not found in scene!");

        if (hotbarPanel == null)
            Debug.LogError("hotbarPanel is NULL!");

        if (slotPrefab == null)
            Debug.LogError("slotPrefab is NULL!");

        EnsureSlotCount();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current != null && Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                UseItemInSlot(i);
            }
        }
    }

    void UseItemInSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }



    public bool GetHotberItem(GameObject itemPrefab)
    {
        foreach (Transform slotTransform in hotbarPanel.transform)
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

    private void EnsureSlotCount()
    {
        // אם יש פחות מדי סלוטים — תיצור
        while (hotbarPanel.transform.childCount < slotCount)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        // אם יש יותר מדי — תמחק
        while (hotbarPanel.transform.childCount > slotCount)
        {
            DestroyImmediate(
                hotbarPanel.transform
                .GetChild(hotbarPanel.transform.childCount - 1)
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

        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Transform slotTransform = hotbarPanel.transform.GetChild(i);
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
    public void SetHotbarItems(List<InventorySaveData> savedItems)
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

            Transform slotTransform = hotbarPanel.transform.GetChild(data.slotIndex);
            Slot slot = slotTransform.GetComponent<Slot>();

            GameObject itemInstance = Instantiate(itemPrefab, slotTransform);
            itemInstance.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            slot.currentItem = itemInstance;
        }
    }

    private void ClearAllSlots()
    {
        foreach (Transform child in hotbarPanel.transform)
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

