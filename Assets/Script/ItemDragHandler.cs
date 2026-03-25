using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform hotbarRect;

    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;

    [SerializeField] LayerMask collisionMask;
    [SerializeField] float checkRadius = 0.3f;
    [SerializeField] int maxAttempts = 15;

    private InventoryController inventoryController;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.instance;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();

        if (dropSlot == null)
        {
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
                dropSlot = dropItem.GetComponentInParent<Slot>();
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if (draggedItem.ID == targetItem.ID)
                {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                    return;
                }
                else
                {
                    GameObject swappedItem = dropSlot.currentItem;
                    swappedItem.transform.SetParent(originalSlot.transform);
                    swappedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    originalSlot.currentItem = swappedItem;
                    dropSlot.currentItem = null;
                }
            }
            else
            {
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);
            transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            dropSlot.currentItem = gameObject;
        }
        else
        {
            // ✅ Check if dropped inside hotbar or inventory
            bool insideHotbar = hotbarRect != null &&
                RectTransformUtility.RectangleContainsScreenPoint(hotbarRect, eventData.position);

            bool insideInventory = IsWithinInventory(eventData.position);

            if (!insideInventory && !insideHotbar)
            {
                // ✅ Only drop to ground if truly outside ALL UI
                DropItem(originalSlot, eventData.position);
                return;
            }

            // ✅ Dropped on hotbar background or inventory background — return to original slot
            ReturnToOriginalSlot(originalSlot);
        }
    }

    // ✅ New method — safely returns item to its original slot
    private void ReturnToOriginalSlot(Slot originalSlot)
    {
        transform.SetParent(originalParent);
        transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // Make sure the slot reference is restored
        if (originalSlot != null)
            originalSlot.currentItem = gameObject;
    }

    bool IsWithinInventory(Vector2 mousePosition)
    {
        // ✅ Null check so it doesn't crash if item came from hotbar
        if (originalParent == null || originalParent.parent == null)
            return false;

        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        if (inventoryRect == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    void DropItem(Slot originalSlot, Vector2 mousePosition)
    {
        Item item = GetComponent<Item>();
        int quantityToDrop = item.quantity;
        originalSlot.currentItem = null;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Missing Player Tag");
            ReturnToOriginalSlot(originalSlot);
            return;
        }

        Transform playerTransform = playerObject.transform;
        Camera cam = Camera.main;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y,
            cam.WorldToScreenPoint(playerTransform.position).z)
        );

        Vector2 direction = (mouseWorld - playerTransform.position).normalized;
        float mouseDistance = Vector2.Distance(playerTransform.position, mouseWorld);
        float distance = Mathf.Clamp(mouseDistance, minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + direction * distance;

        int attempts = 0;
        while (Physics2D.OverlapCircle(dropPosition, checkRadius, collisionMask) != null && attempts < maxAttempts)
        {
            dropPosition += Random.insideUnitCircle * 0.3f;
            attempts++;
        }

        GameObject droppedObject = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = droppedObject.GetComponent<Item>();
        droppedItem.quantity = quantityToDrop;
        droppedItem.UpdateQuantityDisplay();

        BounceEffect bounce = droppedObject.GetComponent<BounceEffect>();
        if (bounce != null)
            bounce.StartBounce();

        Destroy(gameObject);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            SplitStack();
    }

    private void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount);
        GameObject newItem = item.CloneItem(splitAmount);

        if (inventoryController == null || newItem == null)
        {
            item.AddToStack(splitAmount);
            return;
        }

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }

        item.AddToStack(splitAmount);
        Destroy(newItem);
    }
}