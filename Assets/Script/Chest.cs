using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool IsOpened { get; private set; }
    public string ChestID { get; private set; }
    public GameObject itemPrefab;
    public Sprite openedSprite;

    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqeID(gameObject);
    }

    public bool CanInteract()
    {
        return !IsOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenChest(); // תיקנתי לך קצת את שגיאת הכתיב כאן מ-OpenChst
    }

    private void OpenChest()
    {
        // 1. קודם כל משמיעים את הסאונד (רק כשהשחקן באמת פותח)
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play("Chest");
        }

        // 2. משנים את המצב שלה לפתוח
        SetOpened(true);

        // 3. זורקים את החפץ
        if (itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    // הפונקציה הזו משמשת גם את מערכת הטעינה, לכן אין בה סאונד
    public void SetOpened(bool opened)
    {
        IsOpened = opened; // הדרך הנכונה לעדכן משתנה

        if (IsOpened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}