using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    [Header("Settings")]
    public Vector3 iconOffset = new Vector3(0, 1.2f, 0);

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && interactableInRange != null)
        {
            // ביצוע האינטראקציה (פתיחת התיבה)
            interactableInRange.Interact();

            // בדיקה: אם אחרי הלחיצה האובייקט כבר לא ניתן לאינטראקציה (כי התיבה פתוחה)
            // נעלים את האייקון מיד
            if (!interactableInRange.CanInteract())
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;

            // הצבת האייקון מעל התיבה
            interactionIcon.transform.position = collision.transform.position + iconOffset;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}