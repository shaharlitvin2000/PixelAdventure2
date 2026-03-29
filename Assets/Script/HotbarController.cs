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
            hotbarKeys[i] = i < 6 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;

        if (itemDictionary == null)
            Debug.LogError("ItemDictionary not found in scene!");

        if (hotbarPanel == null)
            Debug.LogError("hotbarPanel is NULL!");

        if (slotPrefab == null)
            Debug.LogError("slotPrefab is NULL!");

        EnsureSlotCount();
    }

    void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current != null && Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
                UseItemInSlot(i);
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

    public bool GetHotberItem(GameObject itemPrefab, int quantity = 1)
    {
        Item incomingItem = itemPrefab.GetComponent<Item>();

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item existingItem = slot.currentItem.GetComponent<Item>();
                if (existingItem != null && existingItem.ID == incomingItem.ID)
                {
                    existingItem.AddToStack(quantity);
                    return true;
                }
            }
        }

        foreach (Transform slotTransform in hotbarPanel.transform)
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
                return true;
            }
        }

        Debug.Log("Hotbar is full!");
        return false;
    }

    public void RemoveItemFromHotbar(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            if (amountToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
    }

    private void EnsureSlotCount()
    {
        while (hotbarPanel.transform.childCount < slotCount)
            Instantiate(slotPrefab, hotbarPanel.transform);

        while (hotbarPanel.transform.childCount > slotCount)
            DestroyImmediate(
                hotbarPanel.transform
                .GetChild(hotbarPanel.transform.childCount - 1)
                .gameObject
            );
    }

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
                    slotIndex = i,
                    quantity = item.quantity
                });
            }
        }

        return saveData;
    }

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

            Item itemComponent = itemInstance.GetComponent<Item>();
            if (itemComponent != null)
            {
                itemComponent.quantity = data.quantity;
                itemComponent.UpdateQuantityDisplay();
            }

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