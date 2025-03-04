using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float knockbackDuration = 1f;
    [SerializeField] private float knockbackForce = 10f;
    
    // Animation parameters
    [Header("Animation Parameters")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string directionXParam = "DirectionX";
    [SerializeField] private string directionYParam = "DirectionY";
    [SerializeField] private string isMovingParam = "IsMoving";

    [Header("Camera Settings")]
    [SerializeField] private bool shouldCameraFollow = true;
    [SerializeField] private float cameraFollowSpeed = 5f;
    [SerializeField] private Vector2 cameraOffset = Vector2.zero;
    [SerializeField] private bool useSmoothing = true;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerDash playerDash;
    private PlayerAttack playerAttack;
    public bool isKnockedBack = false;
    private Vector2 facingDirection = Vector2.right;
    private bool isHorizontalRestriction = false;
    private bool isMovementRestricted = false;
    private float isDiagonal = 1f;
    private Camera mainCamera;
    AudioManager audioManager;

    // Animator reference
    private Animator animator;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        rb = GetComponent<Rigidbody2D>();
        playerDash = GetComponent<PlayerDash>();
        playerAttack = GetComponent<PlayerAttack>();
        mainCamera = Camera.main;

        // Get the Animator component
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (playerAttack.canAttack == true)
        {
            HandleNormalMovement();
        }

        if(shouldCameraFollow && mainCamera != null)
        {
            UpdateCameraPosition();
        }
    }

     private void UpdateCameraPosition()
    {
        Vector3 targetPosition = transform.position;
        targetPosition.z = mainCamera.transform.position.z; // Keep the camera's z position
        targetPosition.x += cameraOffset.x;
        targetPosition.y += cameraOffset.y;

        if (useSmoothing)
        {
            // Smoothly move the camera towards the target position
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position, 
                targetPosition, 
                cameraFollowSpeed * Time.deltaTime
            );
        }
        else
        {
            // Instantly set the camera position
            mainCamera.transform.position = targetPosition;
        }
    }

    private void HandleNormalMovement()
    {
        if (isKnockedBack) return;
        
        Vector2 finalMoveInput = moveInput;

        if (isMovementRestricted)
        {
            // Restrict movement based on interaction direction
            if (isHorizontalRestriction)
            {
                finalMoveInput.y = 0; // Only allow horizontal movement
            }
            else
            {
                finalMoveInput.x = 0; // Only allow vertical movement
            }
        }
        else if (moveInput.x != 0 && moveInput.y != 0)
        {
            isDiagonal = 0.75f;
        }
        else
        {
            isDiagonal = 1f;
        }

        if (playerDash != null && !playerDash.IsDashing() && !isKnockedBack)
        {
            rb.velocity = finalMoveInput * moveSpeed * isDiagonal;
            UpdateAnimation();
        }
    }

    private void UpdateAnimation()
    {
        // Update animation parameters for blend trees
        float movementMagnitude = rb.velocity.magnitude;
        
        // Set movement parameters
        animator.SetFloat(speedParam, movementMagnitude);
        animator.SetBool(isMovingParam, movementMagnitude > 0.1f);
        
        // Use moveInput for direction when moving, otherwise use last facing direction
        if (moveInput.magnitude > 0.1f)
        {
            animator.SetFloat(directionXParam, moveInput.x);
            animator.SetFloat(directionYParam, moveInput.y);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        audioManager.PlaySFX(audioManager.playerWalk);
    }

    public Vector2 GetFacingDirection()
    {
        return facingDirection;
    }

    public void SetMovementRestriction(bool isHorizontal)
    {
        isMovementRestricted = true;
        isHorizontalRestriction = isHorizontal;
    }

    public void ClearMovementRestriction()
    {
        isMovementRestricted = false;
    }

    public void ApplyKnockback(Vector2 knockbackDirection)
    {
        isKnockedBack = true;
        rb.velocity = Vector2.zero;
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        Invoke("EndKnockback", knockbackDuration);
    }

    private void EndKnockback()
    {
        isKnockedBack = false;
        rb.velocity = Vector2.zero;
    }
}