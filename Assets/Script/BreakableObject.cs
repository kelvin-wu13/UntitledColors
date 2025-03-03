using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableObject : MonoBehaviour
{
    [Header("Breakable Settings")]
    public int hitPoints = 1;
    public GameObject potionPrefab;
    public bool containsPotion = true;
    
    // [Header("Visual Effects")]
    // public GameObject breakEffect;
    // public AudioClip breakSound;
    
    private int currentHitPoints;
    
    void Start()
    {
        currentHitPoints = hitPoints;

        // Make sure the Breakable layer exists in your project
        gameObject.layer = LayerMask.NameToLayer("Breakable");
    }
    
    // This method should be called when the player's attack hits the object
    public void TakeDamage(int damage)
    {
        currentHitPoints -= damage;
        Debug.Log($"Object took {damage} damage. Remaining HP: {currentHitPoints}");
        
        if (currentHitPoints <= 0)
        {
            Break();
        }
        // else
        // {
        //     Optional: Play hit effect/sound for partial damage
        //     PlayHitEffect();
        // }
    }
    
    private void Break()
    {
        // Spawn break effect if assigned
        // if (breakEffect != null)
        // {
        //     Instantiate(breakEffect, transform.position, Quaternion.identity);
        // }
        
        // Play break sound if assigned
        // if (breakSound != null)
        // {
        //     AudioSource.PlayClipAtPoint(breakSound, transform.position);
        // }
        
        // Spawn potion based on drop rate
        if (containsPotion && potionPrefab != null)
        {
            Instantiate(potionPrefab, transform.position, Quaternion.identity);
            Debug.Log("Spawned a potion!");
        }
        
        // Destroy this breakable object
        Debug.Log("Breaking object!");
        Destroy(gameObject);
    }

    private void PlayHitEffect()
    {
        // Optional: Add visual/audio feedback when object is hit but not broken
        // For example, you could play a particle effect or sound here
    }
}