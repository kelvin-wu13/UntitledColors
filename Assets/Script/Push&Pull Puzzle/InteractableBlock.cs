using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBlock : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float interactionDistance = 1.5f;
    public float fallGravity = 9.8f;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public float groundCheckDistance = 0.2f;
    public float fallDelay = 0.2f;
    public float blockHeight = 1f; // Height of block for platform collision
    
    private bool isBeingMoved = false;
    private bool isFalling = false;
    private bool hasLanded = false;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 interactionDirection;
    private BoxCollider2D boxCollider;
    private int originalLayer;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.drag = 10f;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(1f, 1f);
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        originalLayer = gameObject.layer;
    }

    private void Update()
    {
        if (!isBeingMoved && !isFalling && !hasLanded)
        {
            CheckGroundBelow();
        }

        if (isFalling)
        {
            HandleFalling();
        }
        
        if (hasLanded)
        {
            CheckPlayerOnTop();
        }
    }

    private void CheckGroundBelow()
    {
        // Check if the block is on ground by using multiple raycasts
        bool isGrounded = IsBlockGrounded();
        
        if (!isGrounded)
        {
            // No ground below, start falling after delay
            StartCoroutine(StartFallingAfterDelay());
        }
    }
    
    private bool IsBlockGrounded()
    {
        if (boxCollider == null)
        {
            return true; // Safety check
        }
        
        // Get the bounds of the box collider
        Bounds bounds = boxCollider.bounds;
        
        // Calculate bottom corners of the block
        Vector2 bottomLeft = new Vector2(bounds.min.x + 0.1f, bounds.min.y);
        Vector2 bottomCenter = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 bottomRight = new Vector2(bounds.max.x - 0.1f, bounds.min.y);
        
        // Cast rays from each corner
        bool leftGrounded = Physics2D.Raycast(bottomLeft, Vector2.down, groundCheckDistance, groundLayer);
        bool centerGrounded = Physics2D.Raycast(bottomCenter, Vector2.down, groundCheckDistance, groundLayer);
        bool rightGrounded = Physics2D.Raycast(bottomRight, Vector2.down, groundCheckDistance, groundLayer);
        
        // Debug rays for visualization in Scene view
        Debug.DrawRay(bottomLeft, Vector2.down * groundCheckDistance, leftGrounded ? Color.green : Color.red);
        Debug.DrawRay(bottomCenter, Vector2.down * groundCheckDistance, centerGrounded ? Color.green : Color.red);
        Debug.DrawRay(bottomRight, Vector2.down * groundCheckDistance, rightGrounded ? Color.green : Color.red);
        
        // Consider the block grounded if any of the raycasts hit ground
        return leftGrounded || centerGrounded || rightGrounded;
    }

    private void CheckPlayerOnTop()
    {
        if (boxCollider == null) return;
        
        // Get the bounds of the box collider
        Bounds bounds = boxCollider.bounds;
        
        // Calculate top center of the block
        Vector2 topCenter = new Vector2(bounds.center.x, bounds.max.y + 0.05f);
        
        // Check if player is standing on the block
        Collider2D playerCollider = Physics2D.OverlapCircle(topCenter, 0.5f, playerLayer);
        
        // If player is detected above the block
        if (playerCollider != null)
        {
            // We could add visual feedback here if desired
            Debug.DrawLine(topCenter, playerCollider.transform.position, Color.green);
        }
        gameObject.layer = LayerMask.NameToLayer("Ground");
        boxCollider.isTrigger = true;
    }

    private IEnumerator StartFallingAfterDelay()
    {
        yield return new WaitForSeconds(fallDelay);
        
        // Check again before falling (the block might have moved)
        if (!isBeingMoved && !IsBlockGrounded()) 
        {
            StartFalling();
        }
    }

    private void StartFalling()
    {
        isFalling = true;
        rb.isKinematic = false;
        rb.gravityScale = fallGravity / 9.8f; // Convert to Unity's gravity scale
        
        // Change appearance to indicate falling state
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.8f);
        }
    }

    private void HandleFalling()
    {
        // Check if the block has landed
        if (IsBlockGrounded())
        {
            LandBlock();
        }
    }

    private void LandBlock()
    {
        isFalling = false;
        hasLanded = true;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        
        // Snap to position on landing
        Vector3 pos = transform.position;
        transform.position = new Vector3(
            Mathf.Round(pos.x * 2) / 2,
            Mathf.Round(pos.y * 2) / 2,
            pos.z
        );
        
        // Change to platform state
        ConvertToPlatform();
    }
    
    private void ConvertToPlatform()
    {
        // Change appearance to indicate platform state
        if (spriteRenderer != null)
        {
            // Darken the color slightly to indicate it's now a platform
            spriteRenderer.color = new Color(
                originalColor.r * 0.8f,
                originalColor.g * 0.8f,
                originalColor.b * 0.8f,
                1f
            );
        }
        
        // Ensure the collider is properly set up for platforming
        if (boxCollider != null)
        {
            // Make sure the collider is not a trigger
            boxCollider.isTrigger = false;
            
            // You might want to adjust the collider size for better platforming
            boxCollider.size = new Vector2(boxCollider.size.x, blockHeight);
        }
    }

    public bool CanInteract(Vector2 playerPosition, Vector2 interactDirection)
    {
        // Don't allow interaction if already falling or has landed
        if (isFalling || hasLanded) return false;
        
        Vector2 directionToBlock = ((Vector2)transform.position - playerPosition).normalized;
        float dot = Vector2.Dot(interactDirection, directionToBlock);
        float distance = Vector2.Distance(transform.position, playerPosition);
        
        return distance <= interactionDistance && Mathf.Abs(dot) > 0.9f;
    }


    public void UpdateMovement(Vector2 movement)
    {
        if (!isBeingMoved || isFalling || hasLanded) return;
        rb.velocity = movement;
    }

    public bool IsBeingMoved()
    {
        return isBeingMoved;
    }

    public bool IsFalling()
    {
        return isFalling;
    }

    public bool HasLanded()
    {
        return hasLanded;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (boxCollider == null) return;
        
        Bounds bounds = boxCollider.bounds;
        Vector2 bottomLeft = new Vector2(bounds.min.x + 0.1f, bounds.min.y);
        Vector2 bottomCenter = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 bottomRight = new Vector2(bounds.max.x - 0.1f, bounds.min.y);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(bottomLeft, bottomLeft + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(bottomCenter, bottomCenter + Vector2.down * groundCheckDistance);
        Gizmos.DrawLine(bottomRight, bottomRight + Vector2.down * groundCheckDistance);
        
        // Draw platform detection area
        if (hasLanded)
        {
            Vector2 topCenter = new Vector2(bounds.center.x, bounds.max.y + 0.05f);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(topCenter, 0.5f);
        }
    }
}