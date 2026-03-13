using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float footstepDelay = 0.4f; // כמה זמן לחכות בין צעד לצעד

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    private float footstepTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }

        rb.velocity = moveInput * moveSpeed;
        bool isMoving = rb.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isMoving);

        // מנגנון השמעת צעדים
        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlayFootstep();
                footstepTimer = footstepDelay; // איפוס הטיימר
            }
        }
    }

    private void PlayFootstep()
    {
        if (SoundEffectManager.Instance != null)
        {
            // זה יפעיל צליל אקראי מהרשימה ששמת ב-Library תחת השם Footsteps
            SoundEffectManager.Instance.Play("Footsteps");
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (PauseController.IsGamePaused) return;

        moveInput = context.ReadValue<Vector2>();

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }
}