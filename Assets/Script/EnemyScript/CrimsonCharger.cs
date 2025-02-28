using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimsonCharger : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask gapLayer;

    private enum State
    {
        Roaming,
        Charging,
        Attacking,
        Stunned
    }

    private State currentState;
    private EnemyPathfinding enemyPathfinding;
    private EnemyDetectionRoaming enemyDetection;
    private Rigidbody2D rb;
    private Transform targetPlayer;
    private Vector2 attackDirection;
    private float currentHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isKnockedBack = false;
    private bool hasDealtDamage = false;
    private bool isStunned = false;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        enemyDetection = GetComponent<EnemyDetectionRoaming>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        // Initialize with data from scriptable object
        if (enemyData != null)
        {
            currentHealth = enemyData.health;
            if (enemyPathfinding != null)
            {
                enemyPathfinding.moveSpeed = enemyData.moveSpeed;
            }
        }
        else
        {
            // Default values if enemyData is not assigned
            currentHealth = 5f;
        }

        // Subscribe to player detection events
        if (enemyDetection != null)
        {
            enemyDetection.OnPlayerDetected += HandlePlayerDetected;
            enemyDetection.OnPlayerLost += HandlePlayerLost;
        }

        currentState = State.Roaming;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when object is destroyed
        if (enemyDetection != null)
        {
            enemyDetection.OnPlayerDetected -= HandlePlayerDetected;
            enemyDetection.OnPlayerLost -= HandlePlayerLost;
        }
    }

    private void HandlePlayerDetected(Transform player)
    {
        targetPlayer = player;
        StartCharging();
    }

    private void HandlePlayerLost()
    {
        // If we're not already roaming or stunned, go back to roaming
        if (currentState != State.Roaming && currentState != State.Stunned)
        {
            ResetToRoaming();
        }
    }

    private void StartCharging()
    {
        // Only start charging if we're in roaming state
        if (currentState != State.Roaming) return;
        currentState = State.Charging;
        
        // Stop movement during charge preparation
        if (enemyPathfinding != null)
        {
            enemyPathfinding.MoveTo(Vector2.zero);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        
        // Change color to indicate charging (red tint)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
        
        StartCoroutine(ChargingCoroutine());
    }

    private IEnumerator ChargingCoroutine()
    {
        // Wait for charge time from enemyData or default to 2 seconds
        float chargeTime = (enemyData != null) ? enemyData.chargeTime : 2f;
        yield return new WaitForSeconds(chargeTime);

        // Make sure we're still in charging state
        if (currentState != State.Charging)
        {
            yield break;
        }

        // Calculate direction to player
        if (targetPlayer != null)
        {
            attackDirection = (targetPlayer.position - transform.position).normalized;
            StartAttack();
        }
        else
        {
            // If player reference is lost during charging, go back to roaming
            ResetToRoaming();
        }
    }

    private void StartAttack()
    {
        currentState = State.Attacking;
        hasDealtDamage = false;  // Reset damage flag for new attack
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        float elapsedTime = 0f;
        float attackDuration = 2f; // Time limit for attack before giving up
        float attackSpeed = 10f; // Attack speed, can be adjusted
        bool hasCollided = false;

        // Disable pathfinding during attack
        if (enemyPathfinding != null)
        {
            enemyPathfinding.enabled = false;
        }
        
        while (elapsedTime < attackDuration && !hasCollided && currentState == State.Attacking)
        {
            // Move in attack direction
            rb.velocity = attackDirection * attackSpeed;
            
            // Check for collisions with player
            Collider2D playerHit = Physics2D.OverlapCircle(transform.position, 1f, playerLayer);
            if (playerHit != null && !hasDealtDamage)
            {
                // Hit player - only process once per attack
                hasDealtDamage = true;
                
                PlayerController player = playerHit.GetComponent<PlayerController>();
                if (player != null)
                {
                    float damage = (enemyData != null) ? enemyData.damage : 1f;
                    
                    // Calculate knockback direction away from the enemy
                    Vector2 knockbackDirection = (player.transform.position - transform.position).normalized;
                    
                    // Apply knockback to player immediately
                    player.ApplyKnockback(knockbackDirection);
                    
                    // Apply damage to player if they have a PlayerStats component
                    PlayerStats playerStats = player.GetComponent<PlayerStats>();
                    if (playerStats != null)
                    {
                        playerStats.TakeDamage(damage);
                    }
                }
                
                // Enemy gets stunned for 2 seconds
                hasCollided = true;
                
                // Stop moving immediately
                rb.velocity = Vector2.zero;
                
                // Enter stunned state for 2 seconds
                StartCoroutine(StunnedForDuration(2f));
            }
            
            // Check for collisions with obstacles or gaps
            Collider2D obstacleHit = Physics2D.OverlapCircle(transform.position, 1f, obstacleLayer);
            Collider2D gapHit = Physics2D.OverlapCircle(transform.position, 1f, gapLayer);
            
            if (obstacleHit != null || gapHit != null)
            {
                hasCollided = true;
                rb.velocity = Vector2.zero;
            }
            
            // Check for collisions with other enemies - using a separate check to avoid layer issues
            Collider2D[] enemyHits = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (Collider2D hit in enemyHits)
            {
                if (hit.gameObject != gameObject && hit.GetComponent<CrimsonCharger>() != null)
                {
                    CrimsonCharger otherCharger = hit.GetComponent<CrimsonCharger>();
                    
                    // Only process if we haven't hit this enemy before in this attack
                    if (!hasDealtDamage)
                    {
                        hasDealtDamage = true;
                        
                        // Both take damage
                        TakeDamage(1);
                        otherCharger.TakeDamage(1);
                        
                        // Calculate knockback directions for both
                        Vector2 knockbackDir = (transform.position - otherCharger.transform.position).normalized;
                        
                        // Apply knockback to both with slightly different timings
                        StartCoroutine(DelayedKnockback(knockbackDir, 0.1f));
                        otherCharger.StartCoroutine(otherCharger.DelayedKnockback(-knockbackDir, 0f));
                    }
                    
                    hasCollided = true;
                    rb.velocity = Vector2.zero;
                    break;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Stop movement immediately
        rb.velocity = Vector2.zero;
        
        // Re-enable pathfinding if it exists
        if (enemyPathfinding != null)
        {
            yield return new WaitForSeconds(0.5f);
            enemyPathfinding.enabled = true;
        }

        if (hasCollided)
        {
            // No need to enter stunned state here, it's handled in StunnedForDuration
        }
        else
        {
            // No collision, go back to roaming
            ResetToRoaming();
        }
    }
    
    private IEnumerator StunnedForDuration(float duration)
    {
        // Enter stunned state
        EnterStunnedState();

        // Wait for the stun duration
        yield return new WaitForSeconds(duration);

        // Exit stunned state
        ExitStunnedState();
    }

    private void ExitStunnedState()
    {
        // Reset the stunned flag
        isStunned = false;

        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // Ensure velocity is zero before starting to roam
        rb.velocity = Vector2.zero;

        // Re-enable pathfinding if it exists
        if (enemyPathfinding != null)
        {
            enemyPathfinding.enabled = true;
        }

        currentState = State.Roaming;

        // Check if the player is still nearby
        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
            float detectionRange = 5f; // Default detection range

            if (enemyDetection != null)
            {
                detectionRange = enemyDetection.detectionRange; // Assuming detectionRange is public or accessible
            }

            if (distanceToPlayer <= detectionRange)
            {
                // Player is still nearby, start charging again
                StartCharging();
            }
            else
            {
                // Player is far, go back to roaming
                ResetToRoaming();
            }
        }
        else
        {
            // No player reference, go back to roaming
            ResetToRoaming();
        }
    }

    public IEnumerator DelayedKnockback(Vector2 direction, float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyKnockback(direction);
    }

     private void EnterStunnedState()
    {
        // Only enter stunned state if not already stunned
        if (isStunned)
        {
            return;
        }

        // Set the stunned flag
        isStunned = true;

        // Set state to stunned
        currentState = State.Stunned;

        // Change color to indicate stunned state (blue tint)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }

        // Ensure velocity is zeroed
        rb.velocity = Vector2.zero;
        
        // Disable pathfinding during stun
        if (enemyPathfinding != null)
        {
            enemyPathfinding.enabled = false;
        }

        // Debug log to verify function is called
        Debug.Log("Enemy entered stunned state");
    }

private IEnumerator StunnedCoroutine()
{
    // Stun duration is fixed at 2 seconds
    float stunTime = 2f;

    // Wait for the full stun duration
    yield return new WaitForSeconds(stunTime);

    // Reset the stunned flag
    isStunned = false;

    // After stun period, check if there's a player nearby
    if (targetPlayer != null)
    {
        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
        float detectionRange = 5f; // Default detection range

        // Try to get detection range from the detection component
        if (enemyDetection != null)
        {
            // Note: You would need to make detectionRange public in EnemyDetectionRoaming
            // or add a getter method to access it
            detectionRange = enemyDetection.detectionRange; // Assuming detectionRange is public or accessible
        }

        if (distanceToPlayer <= detectionRange)
        {
            // Player is still nearby, start charging again
            StartCharging();
        }
        else
        {
            // Player is far, go back to roaming
            ResetToRoaming();
        }
    }
    else
    {
        // No player reference, go back to roaming
        ResetToRoaming();
    }
}


    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // Flash red to indicate damage
        StartCoroutine(DamageFlash());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            
            // Return to appropriate color based on state
            if (currentState == State.Charging)
            {
                spriteRenderer.color = Color.red;
            }
            else if (currentState == State.Stunned)
            {
                spriteRenderer.color = Color.blue;
            }
            else
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    private void Die()
    {
        // Stop all coroutines to prevent any ongoing behavior
        StopAllCoroutines();
        
        // Add any death effects here
        Destroy(gameObject);
    }

    public void ApplyKnockback(Vector2 direction)
    {
        // Prevent applying knockback if already knocked back
        if (isKnockedBack) return;
        
        isKnockedBack = true;
        
        // Use knockBackForce from enemyData or default value
        float force = (enemyData != null) ? enemyData.knockBackForce : 5f;
        
        // Clear current velocity before applying force
        rb.velocity = Vector2.zero;
        rb.AddForce(direction * force, ForceMode2D.Impulse);
        
        // Reset knockback flag after a short duration
        StartCoroutine(ResetKnockback());
    }
    
    private IEnumerator ResetKnockback()
    {
        yield return new WaitForSeconds(0.5f);
        isKnockedBack = false;
        
        // If we're in stunned state, make sure velocity is zeroed
        if (currentState == State.Stunned)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void ResetToRoaming()
    {
        currentState = State.Roaming;
        
        // Reset color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        // Ensure velocity is zero before starting to roam
        rb.velocity = Vector2.zero;
        
        // Re-enable pathfinding if it exists
        if (enemyPathfinding != null)
        {
            enemyPathfinding.enabled = true;
        }
        
        // Tell the detection component to start roaming
        if (enemyDetection != null)
        {
            enemyDetection.StartRoaming();
        }
    }
}