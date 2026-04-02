using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float footstepDelay = 0.4f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveInput = Vector2.down;

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
            StopMovement();
            return;
        }

        HandleAnimation();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        if (PauseController.IsGamePaused) return;

        rb.velocity = moveInput * moveSpeed;
    }

    void HandleAnimation()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        animator.SetBool("isWalking", isMoving);

        if (isMoving)
        {
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);

            lastMoveInput = moveInput;
        }
    }

    void HandleFootsteps()
    {
        if (moveInput.magnitude > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                PlayFootstep();
                footstepTimer = footstepDelay;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void PlayFootstep()
    {
        if (SoundEffectManager.Instance != null)
        {
            SoundEffectManager.Instance.Play("Footsteps");
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (context.canceled)
        {
            StopMovement();
        }
    }

    void StopMovement()
    {
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;

        animator.SetBool("isWalking", false);
        animator.SetFloat("LastInputX", lastMoveInput.x);
        animator.SetFloat("LastInputY", lastMoveInput.y);
    }
}