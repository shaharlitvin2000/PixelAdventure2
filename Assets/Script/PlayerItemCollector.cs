using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController == null)
            Debug.LogError("InventoryController not found in scene!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();
            if (item != null)
            {
                // ✅ Save quantity BEFORE anything touches it
                int quantityOnGround = item.quantity;

                bool itemAdded = inventoryController.AddItem(collision.gameObject, quantityOnGround);
                if (itemAdded)
                {
                    item.ShowPopUp();
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}