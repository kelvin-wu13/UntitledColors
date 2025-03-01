// Update Lever.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    [SerializeField] private Sprite activatedSprite;
    [SerializeField] private Sprite deactivatedSprite;
    
    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private bool canBeActivated = true;
    private PuzzleManager puzzleManager;
    [SerializeField] private float cooldownTime = 1f;

    // Add this to identify which platform this lever controls
    [SerializeField] private MovingPlatform targetPlatform;
    
    // Add collider field to detect hits
    private Collider2D leverCollider;

    private void Awake()
    {
        puzzleManager = GetComponent<PuzzleManager>();
        if (puzzleManager == null)
        {
            puzzleManager = gameObject.AddComponent<PuzzleManager>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        leverCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on lever!");
        }
        
        if (leverCollider == null)
        {
            Debug.LogError("Collider2D component not found on lever!");
        }
        
        // Ensure it starts with the deactivated sprite
        if (deactivatedSprite != null)
        {
            spriteRenderer.sprite = deactivatedSprite;
        }
        
        // Set up the connection to the platform in the Unity Event
        if (puzzleManager != null && targetPlatform != null)
        {
            puzzleManager.onLeverActivated.AddListener(targetPlatform.ActivatePlatform);
        }
    }

    // This method will be called when the hitbox from PlayerAttack collides with the lever
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AttackHitbox>() != null)
        {
            TryActivate();
        }
    }

    // This method should be called by the player's attack system
    public void TryActivate()
    {
        if (!canBeActivated) return;
        
        // Toggle the lever state
        isActivated = !isActivated;
        
        // Update the sprite based on the new state
        UpdateSprite();
        
        // Always trigger the event when the lever is hit
        puzzleManager.onLeverActivated.Invoke();
        
        // Start cooldown to prevent rapid toggling
        StartCoroutine(Cooldown());
    }
    
    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;
        
        if (isActivated && activatedSprite != null)
        {
            spriteRenderer.sprite = activatedSprite;
        }
        else if (!isActivated && deactivatedSprite != null)
        {
            spriteRenderer.sprite = deactivatedSprite;
        }
    }
    
    private IEnumerator Cooldown()
    {
        canBeActivated = false;
        yield return new WaitForSeconds(cooldownTime);
        canBeActivated = true;
    }
}