using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    // Animation parameters
    [Header("Animation Parameters")]
    [SerializeField] private string isDashingParam = "IsDashing";
    [SerializeField] private string directionXParam = "DirectionX";
    [SerializeField] private string directionYParam = "DirectionY";

    private Rigidbody2D rb;
    private PlayerController playerController;
    private PlayerAttack playerAttack;
    private Vector2 dashDirection;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float cooldownTimeLeft;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                EndDash();
            }
        }

        if (!canDash)
        {
            cooldownTimeLeft -= Time.deltaTime;
            if (cooldownTimeLeft <= 0)
            {
                canDash = true;
            }
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!canDash || isDashing || playerController.isKnockedBack) return;

        dashDirection = rb.velocity.normalized;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = playerAttack.GetDirectionToCursor();
        }

        StartDash();

        Debug.Log($"Dashing in direction: {dashDirection}");
    }
    
    private Vector2 GetMouseDirection()
    {
        // Get mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        // Get direction from player to mouse
        return ((Vector2)(mousePos - transform.position)).normalized;
    }
    
    private void StartDash()
    {
        // Interrupt any ongoing attacks
        if (playerAttack != null)
        {
            playerAttack.InterruptAttack();
        }
        
        // Start dash
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        cooldownTimeLeft = dashCooldown;
        
        // Set movement velocity
        rb.velocity = dashDirection * dashSpeed;
        
        // Update animation parameters
        animator.SetBool(isDashingParam, true);
        animator.SetFloat(directionXParam, dashDirection.x);
        animator.SetFloat(directionYParam, dashDirection.y);
        
        Debug.Log($"Dashing in direction: {dashDirection}");
    }
    
    private void EndDash()
    {
        isDashing = false;
        rb.velocity = Vector2.zero;
        animator.SetBool(isDashingParam, false);
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}