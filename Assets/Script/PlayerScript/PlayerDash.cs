using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    private Rigidbody2D rb;
    private PlayerController playerController;
    private Vector2 dashDirection;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTimeLeft;
    private float cooldownTimeLeft;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
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

        if (!canDash || isDashing) return;

        dashDirection = rb.velocity.normalized;

        if (dashDirection == Vector2.zero)
        {
            dashDirection = transform.right;
        }

        // Start dash
        isDashing = true;
        canDash = false;
        dashTimeLeft = dashDuration;
        cooldownTimeLeft = dashCooldown;
        rb.velocity = dashDirection * dashSpeed;

        Debug.Log($"Dashing in direction: {dashDirection}");
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}