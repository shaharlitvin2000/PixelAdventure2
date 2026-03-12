using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{

    public bool IsOpened{get; private set;}
    public string ChestID { get; private set;}
    public GameObject itemPrefab;
    public Sprite openedSprite;

    // Start is called before the first frame update
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
        OpenChst();
    }

    private void OpenChst()
    {
        SetOpened(true);

        if(itemPrefab)
        {
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }


    public void SetOpened(bool opened)
    {
        if (IsOpened = opened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }

    }
}


