// ShopController.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    public static ShopController instance;

    [Header("UI")]
    public GameObject shopPanel;
    public Transform shopInventoryGrid;
    public Transform playerInventoryGrid;
    public GameObject shopSlotPrefab;
    public TMP_Text playerMoneyText;
    public TMP_Text shopTitleText;

    private ItemDictionary itemDictionary;
    private InventoryController inventoryController;
    private ShopNPC currentShop;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
        inventoryController = FindObjectOfType<InventoryController>();
        shopPanel.SetActive(false);

        if (CurrencyController.instance != null)
        {
            CurrencyController.instance.OnGoldChanged += UpdateMoneyDisplay;
            UpdateMoneyDisplay(CurrencyController.instance.GetGold());
        }
    }

    private void UpdateMoneyDisplay(int amount)
    {
        if (playerMoneyText != null)
            playerMoneyText.text = amount.ToString();
    }

    // ─── פתיחה/סגירה ──────────────────────────────────────────
    public void OpenShop(ShopNPC shop)
    {
        currentShop = shop;
        shopPanel.SetActive(true);

        if (shopTitleText != null)
            shopTitleText.text = shop.shopKeeperName + "'s Shop";

        PopulateShopGrid();
        PopulatePlayerGrid();
        PauseController.SetPause(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
        currentShop = null;
        PauseController.SetPause(false);
    }

    // ─── גריד החנות ────────────────────────────────────────────
    private void PopulateShopGrid()
    {
        ClearGrid(shopInventoryGrid);

        Debug.Log($"Shop stock count: {currentShop.GetCurrentStock().Count}");

        foreach (var stockItem in currentShop.GetCurrentStock())
        {
            GameObject prefab = itemDictionary.GetItemPrefab(stockItem.itemID);
            Debug.Log($"ItemID: {stockItem.itemID}, Prefab: {prefab}");
            if (prefab == null) continue;

            Item itemData = prefab.GetComponent<Item>();
            int price = itemData != null ? itemData.buyPrice : 0;

            int capturedItemID = stockItem.itemID;
            int capturedPrice = price;
            int capturedQuantity = stockItem.quantity; // הוסף זה

            GameObject slotObj = Instantiate(shopSlotPrefab, shopInventoryGrid);
            ShopSlot slot = slotObj.GetComponent<ShopSlot>();
            slot.Setup(prefab, price, capturedQuantity, shopInventoryGrid, () => // העבר כמות
                OnBuyItem(capturedItemID, capturedPrice));
        }
    }

    // ─── גריד השחקן ────────────────────────────────────────────
    private void PopulatePlayerGrid()
    {
        ClearGrid(playerInventoryGrid);

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot invSlot = slotTransform.GetComponent<Slot>();
            if (invSlot == null || invSlot.currentItem == null) continue;

            Item itemData = invSlot.currentItem.GetComponent<Item>();
            if (itemData == null) continue;

            int sellPrice = itemData.GetSellPrice();
            int capturedItemID = itemData.ID;

            GameObject slotObj = Instantiate(shopSlotPrefab, playerInventoryGrid);
            ShopSlot slot = slotObj.GetComponent<ShopSlot>();

            slot.Setup(invSlot.currentItem, sellPrice, playerInventoryGrid, () =>
                OnSellItem(capturedItemID, sellPrice));
        }
    }

    // ─── קנייה ─────────────────────────────────────────────────
    private void OnBuyItem(int itemID, int price)
    {
        if (currentShop == null) return;

        if (!CurrencyController.instance.SpendGold(price))
        {
            Debug.Log("Not enough gold!");
            return;
        }

        GameObject prefab = itemDictionary.GetItemPrefab(itemID);
        if (prefab == null) return;

        currentShop.RemoveFromShopStock(itemID, 1);
        inventoryController.AddItem(prefab, 1);

        PopulateShopGrid();
        PopulatePlayerGrid();
    }

    // ─── מכירה ─────────────────────────────────────────────────
    private void OnSellItem(int itemID, int sellPrice)
    {
        inventoryController.RemoveItemFromInventory(itemID, 1);
        CurrencyController.instance.AddGold(sellPrice);
        currentShop.AddToStock(itemID, 1);

        PopulateShopGrid();
        PopulatePlayerGrid();
    }

    // ─── ניקוי גריד ────────────────────────────────────────────
    private void ClearGrid(Transform grid)
    {
        foreach (Transform child in grid)
            Destroy(child.gameObject);
    }
}