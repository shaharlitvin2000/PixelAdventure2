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

    void Update()
    {
        // אם המשחק בעצירה והאייקון דולק - נכבה אותו
        // אם המשחק לא בעצירה ויש תיבה בטווח - נדליק אותו
        if (PauseController.IsGamePaused)
        {
            if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
        }
        else if (interactableInRange != null && interactableInRange.CanInteract())
        {
            if (!interactionIcon.activeSelf) interactionIcon.SetActive(true);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // בדיקה: אם המשחק בעצירה, אי אפשר לפתוח תיבות
        if (PauseController.IsGamePaused) return;

        if (context.performed && interactableInRange != null)
        {
            interactableInRange.Interact();

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

            interactionIcon.transform.position = collision.transform.position + iconOffset;

            // נדליק את האייקון רק אם המשחק לא בעצירה כרגע
            if (!PauseController.IsGamePaused)
            {
                interactionIcon.SetActive(true);
            }
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