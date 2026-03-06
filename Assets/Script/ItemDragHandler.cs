using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    [SerializeField] private RectTransform hotbarRect;

    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
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
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            if (dropSlot.currentItem != null)
            {
                dropSlot.currentItem.transform.SetParent(originalSlot.transform);
                dropSlot.currentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                originalSlot.currentItem = dropSlot.currentItem;
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
            bool insideInventory = IsWithinInventory(eventData.position);

            bool insideHotbar = RectTransformUtility.RectangleContainsScreenPoint(
                hotbarRect,
                eventData.position
            );

            if (!insideInventory && !insideHotbar)
            {
                DropItem(originalSlot, eventData.position);
                return;
            }

            transform.SetParent(originalParent);
            transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();

        return RectTransformUtility.RectangleContainsScreenPoint(
            inventoryRect,
            mousePosition
        );
    }

    [SerializeField] LayerMask collisionMask;
    [SerializeField] float checkRadius = 2f;
    [SerializeField] int maxAttempts = 15;

    void DropItem(Slot originalSlot, Vector2 mousePosition)
    {
        originalSlot.currentItem = null;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("Missing Player Tag");
            return;
        }

        Transform playerTransform = playerObject.transform;

        Camera cam = Camera.main;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(
            new Vector3(mousePosition.x, mousePosition.y,
            cam.WorldToScreenPoint(playerTransform.position).z)
        );

        Vector2 direction = (mouseWorld - playerTransform.position).normalized;

        float distance = Random.Range(minDropDistance, maxDropDistance);

        Vector2 dropPosition = (Vector2)playerTransform.position + direction * distance;

        int attempts = 0;

        // אם יש collider במקום – ננסה להזיז עד שמוצאים מקום פנוי
        while (Physics2D.OverlapCircle(dropPosition, checkRadius, collisionMask) != null && attempts < maxAttempts)
        {
            dropPosition += Random.insideUnitCircle * 0.5f;
            attempts++;
        }

        Instantiate(gameObject, dropPosition, Quaternion.identity);

        Destroy(gameObject);
    }
}