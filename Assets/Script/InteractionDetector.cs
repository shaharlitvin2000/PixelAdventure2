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
        // FIX: null guard — warns you clearly instead of spamming UnassignedReferenceException
        if (interactionIcon == null)
        {
            Debug.LogError("InteractionDetector: 'interactionIcon' is not assigned in the Inspector!", this);
            return;
        }

        interactionIcon.SetActive(false);
    }

    void Update()
    {
        // FIX: null guard at top of Update — stops all spam if icon isn't assigned
        if (interactionIcon == null) return;

        if (PauseController.IsGamePaused)
        {
            if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
        }
        else if (interactableInRange != null)
        {
            MonoBehaviour mb = interactableInRange as MonoBehaviour;
            if (mb != null)
            {
                if (interactableInRange.CanInteract())
                {
                    if (!interactionIcon.activeSelf) interactionIcon.SetActive(true);
                    interactionIcon.transform.position = mb.transform.position + iconOffset;
                }
                // FIX: if CanInteract() is false, hide the icon
                else
                {
                    if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
                }
            }
            else
            {
                interactableInRange = null;
                if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
            }
        }
        // FIX: if nothing is in range, make sure icon is hidden
        else
        {
            if (interactionIcon.activeSelf) interactionIcon.SetActive(false);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (PauseController.IsGamePaused) return;
        if (interactionIcon == null) return; // FIX: null guard

        if (context.performed && interactableInRange != null)
        {
            MonoBehaviour mb = interactableInRange as MonoBehaviour;
            if (mb != null)
            {
                interactableInRange.Interact();
                if (!interactableInRange.CanInteract())
                    interactionIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (interactionIcon == null) return; // FIX: null guard

        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.transform.position = collision.transform.position + iconOffset;
            if (!PauseController.IsGamePaused)
                interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null || collision.gameObject == null) return;
        if (interactionIcon == null) return; // FIX: null guard

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