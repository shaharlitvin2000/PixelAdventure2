using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ItemPickupUIController : MonoBehaviour
{
    public static ItemPickupUIController Instance { get; private set; }

    [SerializeField] private GameObject popupPrefab;
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float popupDuration = 3f;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private List<GameObject> activePopups = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create popup pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject popup = Instantiate(popupPrefab, transform);
            popup.SetActive(false);
            pool.Enqueue(popup);
        }
    }

    public void ShowItemPickup(string itemName, Sprite icon)
    {
        GameObject popup;

        if (pool.Count > 0)
        {
            popup = pool.Dequeue();
        }
        else
        {
            // Reuse oldest popup
            popup = activePopups[0];
            activePopups.RemoveAt(0);

            popup.SetActive(false);
        }

        popup.SetActive(true);

        TMP_Text text = popup.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = itemName;

        Image img = popup.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (img != null)
            img.sprite = icon;

        CanvasGroup canvas = popup.GetComponent<CanvasGroup>();
        if (canvas == null)
            canvas = popup.AddComponent<CanvasGroup>();

        canvas.alpha = 1f;

        activePopups.Add(popup);

        // Force layout update so popups stack correctly
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);

        StartCoroutine(FadeAndReturn(popup, canvas));
    }

    private IEnumerator FadeAndReturn(GameObject popup, CanvasGroup canvas)
    {
        yield return new WaitForSeconds(popupDuration);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            canvas.alpha = 1f - t;
            yield return null;
        }

        popup.SetActive(false);

        if (activePopups.Contains(popup))
            activePopups.Remove(popup);

        pool.Enqueue(popup);

        // Refresh layout after removal
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
    }
}