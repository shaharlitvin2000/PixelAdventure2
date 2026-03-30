using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public int quantity = 1;

    private TMP_Text quantityText;

    private void Awake()
    {
        quantityText = GetComponentInChildren<TMP_Text>();

        if (quantityText == null)
            Debug.LogWarning($"[Item] No TMP_Text found in children of '{gameObject.name}'");
        else
            UpdateQuantityDisplay();
    }

    public void UpdateQuantityDisplay()
    {
        // FIX: Guard against null quantityText
        if (quantityText == null)
            quantityText = GetComponentInChildren<TMP_Text>();

        if (quantityText != null)
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        else
            Debug.LogWarning($"[Item] quantityText is null on '{gameObject.name}'");
    }

    // Renamed from AddToStuck — kept overload for flexibility
    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    // Renamed from RemoveFromStuck
    public int RemoveFromStack(int amount = 1)
    {
        int removed = Mathf.Min(amount, quantity);
        quantity -= removed;
        UpdateQuantityDisplay();
        return removed;
    }

    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item clonedItem = clone.GetComponent<Item>();
        clonedItem.quantity = newQuantity;
        clonedItem.UpdateQuantityDisplay();
        return clone;
    }

    public virtual void UseItem()
    {
        Debug.Log("Using item: " + Name);
    }

    public virtual void ShowPopUp()
    {
        Sprite itemIcon = GetComponent<Image>()?.sprite;
        if (ItemPickupUIController.Instance != null)
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
    }
}