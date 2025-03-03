using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public float damage;
    public float lifetime = 1f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug logging to help troubleshoot
        Debug.Log($"AttackHitbox triggered with: {other.gameObject.name} on layer: {LayerMask.LayerToName(other.gameObject.layer)}");

        // Check for CrimsonCharger (enemy) first
        CrimsonCharger enemy = other.GetComponent<CrimsonCharger>();
        if (enemy != null)
        {
            Debug.Log($"Hit enemy with damage: {damage}");
            enemy.TakeDamage(damage);
            return; // Return early if we hit an enemy
        }

        // Check for BreakableObject next
        BreakableObject breakable = other.GetComponent<BreakableObject>();
        if (breakable != null)
        {
            // Convert damage from float to int (rounding up)
            int damageAmount = Mathf.CeilToInt(damage);
            Debug.Log($"Hit breakable with damage: {damageAmount}");
            breakable.TakeDamage(damageAmount);
        }
    }
}