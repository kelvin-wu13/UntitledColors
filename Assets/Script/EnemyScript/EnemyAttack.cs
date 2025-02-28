using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float attackRange = 5.0f; // Range at which the enemy will attack
    public float attackCooldown = 2.0f; // Time between attacks
    public float lungeSpeed = 5.0f; // Speed of the lunge attack
    public float lungeDistance = 2.0f; // Distance the enemy will lunge
    //public Animator animator; // Reference to the enemy's animator (optional)

    private float timeSinceLastAttack = 0.0f;
    private bool isAttacking = false;

    void Update()
    {
        // Calculate the distance to the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if the player is within attack range and the enemy is not already attacking
        if (distanceToPlayer <= attackRange && !isAttacking)
        {
            // Reset the attack timer
            timeSinceLastAttack = 0.0f;

            // Trigger the lunge attack
            StartCoroutine(LungeAttack());
        }
        else
        {
            // Increment the attack timer
            timeSinceLastAttack += Time.deltaTime;
        }
    }

    private System.Collections.IEnumerator LungeAttack()
    {
        // Set the attacking flag to true
        isAttacking = true;

        // Trigger the attack animation (if using Animator)
        //if (animator != null)
        //{
        //    animator.SetTrigger("Attack");
        //}

        yield return new WaitForSeconds(2.0f);

        // Calculate the direction towards the player
        Vector2 direction = (player.position - transform.position).normalized;

        // Move the enemy towards the player
        float lungeTime = lungeDistance / lungeSpeed;
        float elapsed = 0.0f;
        while (elapsed < lungeTime)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, lungeSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset the attacking flag
        isAttacking = false;
    }
}