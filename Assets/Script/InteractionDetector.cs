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
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
        }
        else if (interactableInRange != null)
        {
            // בדיקה אם האובייקט נמחק (למשל נאסף לאינוונטורי)
            MonoBehaviour mb = interactableInRange as MonoBehaviour;
            if (mb != null)
            {
                if (interactableInRange.CanInteract())
                {
                    if (!interactionIcon.activeSelf) interactionIcon.SetActive(true);
                }
            }
            else
            {
                // האובייקט כבר לא קיים בסצנה
                interactableInRange = null;
                if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (PauseController.IsGamePaused) return;

        if (context.performed && interactableInRange != null)
        {
            // וודא שהאובייקט עדיין קיים לפני האינטראקציה
            MonoBehaviour mb = interactableInRange as MonoBehaviour;
            if (mb != null)
            {
                interactableInRange.Interact();

                if (!interactableInRange.CanInteract())
                {
                    interactionIcon.SetActive(false);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.transform.position = collision.transform.position + iconOffset;

            if (!PauseController.IsGamePaused)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // הבדיקה הקריטית שמונעת את השגיאה האדומה:
        if (collision == null || collision.gameObject == null) return;

        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (interactable == interactableInRange)
            {
                interactableInRange = null;
                interactionIcon.SetActive(false);
            }
        }
    }
}