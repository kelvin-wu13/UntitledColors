using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Properties")]
    [SerializeField] private float comboTimeWindow = 0.5f;
    [SerializeField] private float lightAttackDashDistance = 2f;
    [SerializeField] private float heavyAttackDashDistance = 3f;
    [SerializeField] private float heavyAttackChargeTime = 2f;
    
    [Header("Attack Damage")]
    [SerializeField] private float basicAttackDamage = 1f;
    [SerializeField] private float comboFinisherDamage = 2f;
    [SerializeField] private float heavyAttackDamage = 5f;
    
    [Header("Attack Hitbox")]
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private Vector2 attackSize = new Vector2(1.5f, 1f);
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float hitboxLifetime = 1f;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private static string directionXParam = "DirectionX";
    [SerializeField] private static string directionYParam = "DirectionY";
    [SerializeField] private static string heavyChargingParam = "HeavyCharging";
    [SerializeField] private static string isHeavyAttackParam = "HeavyAttack";
    [SerializeField] private static string comboCountParam = "comboCount"; // Ensure this matches the parameter name in the Animator
    [SerializeField] private static string isAttackingParam = "IsAttacking";
    [SerializeField] private static string isLightAttackParam = "LightAttack";
    [SerializeField] private static string isWaitingComboParam = "IsWaitingCombo";

    public int comboCount = 0;
    private float lastAttackTime;
    private bool isCharging = false;
    private float chargeStartTime;
    public bool canAttack = true;
    private Coroutine currentAttackCoroutine;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private PlayerController playerController;
    private CrimsonCharger enemy;
    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        playerController = GetComponent<PlayerController>();
        
        // If animator not assigned in inspector, try to get component
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Always initialize animation parameters
        SetupAnimationParameters();
    }

    private void Update()
    {
        if (Time.time - lastAttackTime > comboTimeWindow && canAttack)
        {
            comboCount = 0;
            animator.SetInteger(comboCountParam, 0);
            animator.SetBool(isAttackingParam, false);
            animator.SetBool(isWaitingComboParam, false);
        }

        // Always update direction to cursor for responsiveness
        if (Mouse.current != null && Mouse.current.wasUpdatedThisFrame)
        {
            Vector2 direction = GetDirectionToCursor();

            // Update direction parameters when able to attack or when charging
            if (canAttack || isCharging)
            {
                UpdateDirectionParams(direction);
            }
        }

        if (isCharging)
        {
            float chargeProgress = (Time.time - chargeStartTime) / heavyAttackChargeTime;
        }
    }

    // Improved method to update direction parameters
    private void UpdateDirectionParams(Vector2 direction)
    {
        if (animator != null)
        { 
            animator.SetFloat(directionXParam, direction.x);
            animator.SetFloat(directionYParam, direction.y); 
        }
    }

    public Vector2 GetDirectionToCursor()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        Vector3 direction = (mousePos - transform.position).normalized;
        return (Vector2)direction;
    }

    private void DealDamageInArea(Vector2 center, float damage)
    {
        // Create a hitbox at the attack position
        GameObject hitbox = Instantiate(hitboxPrefab, center, Quaternion.identity);

        // Set the hitbox's damage and lifetime
        AttackHitbox hitboxComponent = hitbox.GetComponent<AttackHitbox>();
        if (hitboxComponent != null)
        {
            hitboxComponent.damage = damage;
            hitboxComponent.lifetime = hitboxLifetime;
        }

        // Rotate the hitbox to face the attack direction
        Vector2 direction = GetDirectionToCursor();
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        hitbox.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Size the collider
        BoxCollider2D collider = hitbox.GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = attackSize;
        }

        // Move the hitbox with the player's attack direction
        StartCoroutine(MoveHitboxWithAttack(hitbox, direction));
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || playerController.isKnockedBack) return;

        if (context.performed)
        {
            // Stop any current attack
            if (currentAttackCoroutine != null)
            {
                StopCoroutine(currentAttackCoroutine);
            }

            // Start the attack coroutine
            currentAttackCoroutine = StartCoroutine(PerformLightAttack());
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || playerController.isKnockedBack) return;

        if (context.started)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            animator.SetBool(heavyChargingParam, true);
            audioManager.PlaySFX(audioManager.chargeHeavy);
            Debug.Log("Charging heavy attack...");

            Vector2 direction = GetDirectionToCursor();
            UpdateDirectionParams(direction);
        }
        else if (context.canceled && isCharging)
        {
            isCharging = false;
            animator.SetBool(heavyChargingParam, false);
            animator.SetBool(isLightAttackParam, false);
            
            float chargeTime = Time.time - chargeStartTime;
            if (chargeTime >= heavyAttackChargeTime)
            {
                currentAttackCoroutine = StartCoroutine(PerformHeavyAttack());
            }
            else
            {
                Debug.Log($"Heavy attack canceled - Charge progress: {(chargeTime / heavyAttackChargeTime * 100):F0}%");
            }
        }
    }

    private IEnumerator PerformLightAttack()
    {
        canAttack = false;

        // Get latest direction for attack and update animation parameters
        Vector2 attackDirection = GetDirectionToCursor();
        
        UpdateDirectionParams(attackDirection);

        // Store the current combo for this attack
        int currentCombo = comboCount;
        
        // Set attacking state
        animator.SetBool(isAttackingParam, true);
        animator.SetBool(isLightAttackParam, true);
        animator.SetBool(isWaitingComboParam, false);

        
        animator.SetInteger(comboCountParam, currentCombo);

        // Visual feedback and attack logic based on combo count
        float attackDuration = 0.5f;
        float dashDistance;
        float damage;

        switch (currentCombo)
        {
            case 0:audioManager.PlaySFX(audioManager.playerLightAttack1);
                attackDuration = 0.5f;
                dashDistance = lightAttackDashDistance;
                damage = basicAttackDamage;
                break;
            case 1:audioManager.PlaySFX(audioManager.playerLightAttack2);
                attackDuration = 0.5f;
                dashDistance = lightAttackDashDistance;
                damage = basicAttackDamage;
                break;
            default:
            audioManager.PlaySFX(audioManager.playerLightAttack3);
                attackDuration = 0.5f;
                dashDistance = lightAttackDashDistance * 3f;
                damage = comboFinisherDamage;
                break;
        }

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + (attackDirection * dashDistance);
        
        float elapsedTime = 0f;
        float dashDuration = attackDuration;
        dashDuration *= 0.5f;

        // Deal damage at the midpoint of the dash
        bool damageDealt = false;


        while (elapsedTime < dashDuration)
        {
            float t = elapsedTime / dashDuration;
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            rb.MovePosition(currentPos);

            // Deal damage when we're halfway through the dash
            if (!damageDealt && t >= 0.5f)
            {
                DealDamageInArea(currentPos, damage);
                damageDealt = true;
            }

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        
        // Deal damage at the end if we somehow missed the midpoint
        if (!damageDealt)
        {
            DealDamageInArea(targetPos, damage);
        }
        
        // Wait for the attack animation to complete
        yield return new WaitForSeconds(attackDuration - dashDuration);

        // IMPORTANT: Update the combo count AFTER the current attack is finished
        lastAttackTime = Time.time;
        comboCount = (comboCount + 1) % 3;

        animator.SetBool(isWaitingComboParam, true);

        // Reset attack state
        canAttack = true;
    }

    private IEnumerator PerformHeavyAttack()
    {
        canAttack = false;

        // Get and update attack direction right before triggering animation
        Vector2 attackDirection = GetDirectionToCursor();
        UpdateDirectionParams(attackDirection);

        // Set the attacking state
        animator.SetBool(isAttackingParam, true);
        animator.SetBool(isHeavyAttackParam, true);

        audioManager.PlaySFX(audioManager.playerHeavyAttack);
        

        // Wait a frame to ensure the animation system processes the trigger
        yield return null;

        Debug.Log("Performing Heavy Attack!");

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + (attackDirection * heavyAttackDashDistance);
        
        float elapsedTime = 0f;
        float dashDuration = 0.3f;

        bool damageDealt = false;

        while (elapsedTime < dashDuration)
        {
            float t = elapsedTime / dashDuration;
            Vector2 currentPos = Vector2.Lerp(startPos, targetPos, t);
            rb.MovePosition(currentPos);

            // Deal damage when we're halfway through the dash
            if (!damageDealt && t >= 0.5f)
            {
                DealDamageInArea(currentPos, heavyAttackDamage);
                damageDealt = true;
            }

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        
        // Deal damage at the end if we somehow missed the midpoint
        if (!damageDealt)
        {
            DealDamageInArea(targetPos, heavyAttackDamage);
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool(isAttackingParam, false);
        canAttack = true;
    }

    private void SetupAnimationParameters()
    {
        if (animator != null)
        {
            // Reset all animation parameters to default state
            animator.SetInteger(comboCountParam, 0);
            animator.SetBool(isAttackingParam, false);
            animator.SetBool(heavyChargingParam, false);
            
            // Initialize direction to face forward
            animator.SetFloat(directionXParam, 1);
            animator.SetFloat(directionYParam, 0);
            
            Debug.Log("Animation parameters initialized");
        }
    }

    public void InterruptAttack()
    {
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }
        
        isCharging = false;
        animator.SetBool(heavyChargingParam, false);
        animator.SetBool(isAttackingParam, false);
        canAttack = true;
        Debug.Log("Attack interrupted!");
    }

     private IEnumerator MoveHitboxWithAttack(GameObject hitbox, Vector2 direction)
    {
        float elapsedTime = 0f;
        float moveDuration = 0.2f; // Duration for the hitbox to move with the attack

        Vector2 startPos = hitbox.transform.position;
        Vector2 targetPos = startPos + (direction * lightAttackDashDistance); // Adjust based on attack type

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            hitbox.transform.position = Vector2.Lerp(startPos, targetPos, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the hitbox reaches the target position
        hitbox.transform.position = targetPos;
    }
}