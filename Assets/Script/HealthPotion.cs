using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPotion : MonoBehaviour
{
    [Header("Potion Settings")]
    public int maxPotions = 3;
    public int currentPotions;
    
    private PlayerStats playerStats;
    private UIManager uiManager;

    //Detect nearby potions
    private float collectRadius = 2f;
    private LayerMask potionLayer;
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        uiManager = UIManager.instance;

        // Initialize potions
        uiManager.currentPotions = currentPotions;
        uiManager.UpdatePotionDisplay();

        //Set potion Layer for pickup
        potionLayer = LayerMask.GetMask("Potion");
    }
    
    void Update()
    {
        // Check for potion use input
        if (Input.GetKeyDown(KeyCode.E) && currentPotions > 0)
        {
            UsePotion();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CollectPotion();
        }
    }
    
    public void UsePotion()
    {
        if (currentPotions <= 0 || playerStats.currentHealth >= playerStats.maxHealth)
        {
            return;
        }

        // Decrease potion count
        currentPotions--;
        
        // Heal player to max health
        playerStats.currentHealth = playerStats.maxHealth;
        
        // Update the UI
        uiManager.currentPotions = currentPotions;
        uiManager.UpdatePotionDisplay();
        uiManager.UpdatePlayerHealthUI();
            
        //     // Play effects (optional)
        //     //PlayPotionEffects();
        // }
    }
    
    public void AddPotion()
    {
        if (currentPotions < maxPotions)
        {
            currentPotions++;

            uiManager.currentPotions = currentPotions;
            uiManager.UpdatePotionDisplay();
            Debug.Log("Potion Added");
        }
    }

    private void CollectPotion()
    {
        // Check for nearby potion items
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, collectRadius, potionLayer);
        
        foreach (Collider2D collider in hitColliders)
        {
            // Find PotionPickup component
            PotionObject potionObject = collider.GetComponent<PotionObject>();
            
            if (potionObject != null)
            {
                if (currentPotions < maxPotions)
                {
                    AddPotion();
                    Destroy(collider.gameObject);
                    Debug.Log("Collected a potion!");
                }
                else
                {
                    Debug.Log("Potion inventory full!");
                }
                break;
            }
        }
    }
    
    //private void PlayPotionEffects()
    //{
        // Optional: Add visual or audio effects when using a potion
        // For example:
        // Instantiate(healEffect, playerStats.transform.position, Quaternion.identity);
        // AudioSource.PlayClipAtPoint(healSound, playerStats.transform.position);
    //}
    
    // Method to collect potions in the game world
    // public void CollectPotion()
    // {
    //     if (currentPotions < maxPotions)
    //     {
    //         AddPotion();
    //         return true;
    //     }
    //     return false;
    // }
}