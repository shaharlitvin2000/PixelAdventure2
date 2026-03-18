using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float footstepDelay = 0.4f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveInput = Vector2.down; // שומר כיוון אחרון!
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
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }

        rb.velocity = moveInput * moveSpeed;
        bool isMoving = rb.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0)
            {
                PlayFootstep();
                footstepTimer = footstepDelay;
            }
        }
    }

    private void PlayFootstep()
    {
        if (SoundEffectManager.Instance != null)
            SoundEffectManager.Instance.Play("Footsteps");
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            // שומר את הכיוון האחרון האמיתי — לא האפס!
            animator.SetFloat("LastInputX", lastMoveInput.x);
            animator.SetFloat("LastInputY", lastMoveInput.y);
        }
        else
        {
            // שומר כיוון רק כשזזים בפועל
            lastMoveInput = moveInput;
        }
    }
}