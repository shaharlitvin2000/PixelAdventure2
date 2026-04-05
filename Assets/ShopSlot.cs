// ShopSlot.cs
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    public GameObject currentItem;
    public int itemPrice;
    public TMP_Text priceText;
    public bool isShopSlot = true;

    private Action onClickCallback;

    private void Awake()
    {
        if (!priceText)
            priceText = transform.Find("PriceText")?.GetComponent<TMP_Text>();

        Button btn = GetComponent<Button>();
        if (btn == null) btn = gameObject.AddComponent<Button>();
        btn.onClick.AddListener(() => onClickCallback?.Invoke());
    }

    public void UpdatePriceDisplay()
    {
        if (priceText && currentItem)
            priceText.text = itemPrice.ToString();
    }

    public void SetItem(GameObject item, int price)
    {
        currentItem = item;
        itemPrice = price;
        UpdatePriceDisplay();
    }

    // Setup מלא עם callback לקנייה/מכירה
    public void Setup(GameObject itemPrefab, int price, Transform parent, Action onClick)
    {
        onClickCallback = onClick;
        itemPrice = price;

        // יצירת הפריט בתוך הסלוט
        GameObject itemInstance = Instantiate(itemPrefab, transform);
        RectTransform rect = itemInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        currentItem = itemInstance;
        UpdatePriceDisplay();
    }

    public void Setup(GameObject itemPrefab, int price, int quantity, Transform parent, Action onClick)
    {
        onClickCallback = onClick;
        itemPrice = price;

        GameObject itemInstance = Instantiate(itemPrefab, transform);
        RectTransform rect = itemInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        // הגדר כמות על הפריט
        Item item = itemInstance.GetComponent<Item>();
        if (item != null)
        {
            item.quantity = quantity;
            item.UpdateQuantityDisplay();
        }

        currentItem = itemInstance;
        UpdatePriceDisplay();
    }
}