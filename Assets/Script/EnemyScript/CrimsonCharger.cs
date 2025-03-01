using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrimsonCharger : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask gapLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private readonly int StateParam = Animator.StringToHash("State");
    private readonly int DirectionXParam = Animator.StringToHash("DirectionX");
    private readonly int DirectionYParam = Animator.StringToHash("DirectionY");
    private readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private readonly int SpeedParam = Animator.StringToHash("Speed");
    private readonly int DieParam = Animator.StringToHash("Die");

    private enum AnimState
    {
        Idle = 0,
        Roaming = 1,
        Charging = 2,
        Attacking = 3,
        Stunned = 4,
        Die = 5
    }

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
    private bool isKnockedBack = false;
    private bool hasDealtDamage = false;
    private bool isStunned = false;
    private Vector2 lastMovementDirection;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        enemyDetection = GetComponent<EnemyDetectionRoaming>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        //Register this enemy with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyInCurrentRegion(gameObject);
        }
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
        SetAnimationState(AnimState.Roaming);
        currentState = State.Roaming;
    }

    private void Update()
    {
        UpdateAnimationParameters();
    }

    private void UpdateAnimationParameters()
    {
        // Update movement direction for blend tree
        if (currentState == State.Roaming)
        {
            lastMovementDirection = ((Vector2)transform.position - enemyPathfinding.moveDirection).normalized;
            // Save movement direction for the blend tree
            animator.SetBool(IsMovingParam, true);
            animator.SetFloat(SpeedParam, rb.velocity.magnitude);
        }
        else
        {
            lastMovementDirection = ((Vector2)transform.position - (Vector2)targetPlayer.position).normalized;
            animator.SetBool(IsMovingParam, false);
            animator.SetFloat(SpeedParam, 0f);
        }
        
        // Set direction parameters for blend tree
        animator.SetFloat(DirectionXParam, lastMovementDirection.x);
        animator.SetFloat(DirectionYParam, lastMovementDirection.y);
        
        spriteRenderer.flipX = lastMovementDirection.x < 0;
        
    }

    private void SetAnimationState(AnimState state)
    {
        animator.SetInteger(StateParam, (int)state);
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

        SetAnimationState(AnimState.Charging);
        
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
        // if (spriteRenderer != null)
        // {
        //     spriteRenderer.color = Color.red;
        // }
        
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

        SetAnimationState(AnimState.Attacking);

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

            //Update Movement
            lastMovementDirection = attackDirection;
            
            // Check for collisions with player
            Collider2D playerHit = Physics2D.OverlapArea(transform.position - new Vector3(-2, -1.5f, 0), transform.position - new Vector3(2, 1.5f, 0), playerLayer);
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
                        Debug.Log("Player take 1 damage");
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
            Collider2D obstacleHit = Physics2D.OverlapArea(transform.position - new Vector3(-2, -1.5f, 0), transform.position - new Vector3(2, 1.5f, 0), obstacleLayer);
            Collider2D gapHit = Physics2D.OverlapArea(transform.position - new Vector3(-2, -1.5f, 0), transform.position - new Vector3(2, 1.5f, 0), gapLayer);
            
            if (obstacleHit != null || gapHit != null)
            {
                hasCollided = true;
                rb.velocity = Vector2.zero;
            }
            
            // Check for collisions with other enemies - using a separate check to avoid layer issues
            Collider2D[] enemyHits = Physics2D.OverlapCircleAll(transform.position, 2f);
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

        if (!hasCollided)
        {
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

        // Set state animation to stunned
        currentState = State.Stunned;
        SetAnimationState(AnimState.Stunned);


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
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        //Register Death with GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEnemyDeath(gameObject);
        }

        // Stop all coroutines to prevent any ongoing behavior
        StopAllCoroutines();
        
        // Add any death effects here
        SetAnimationState(AnimState.Die);
        animator.SetTrigger(DieParam);

        //Disable component that might interfere with animation
        enemyPathfinding.enabled = false;
        rb.velocity = Vector2.zero;

        StartCoroutine(DestroyAfterAnimation());
    }

    public void ResetEnemy()
    {
        // Reset enemy state when respawned
        currentHealth = enemyData.health;
        isKnockedBack = false;
        hasDealtDamage = false;
        isStunned = false;
        
        // Reset state
        SetAnimationState(AnimState.Roaming);
        currentState = State.Roaming;
        
        // Re-enable components
        if (enemyPathfinding != null)
        {
            enemyPathfinding.enabled = true;
        }
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(2f);
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
        currentState = State.Roaming;

        UpdateAnimationParameters();

        SetAnimationState(AnimState.Roaming);
    }
}