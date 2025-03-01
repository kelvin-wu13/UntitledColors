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
    
    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        uiManager = UIManager.instance;

        // Initialize potions
        uiManager.currentPotions = currentPotions;
        uiManager.UpdatePotionDisplay();
    }
    
    void Update()
    {
        // Check for potion use input
        if (Input.GetKeyDown(KeyCode.E) && currentPotions > 0)
        {
            UsePotion();
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

        // Heal player
        // float previousHealth = playerStats.currentHealth;
        // playerStats.currentHealth = playerStats.maxHealth;
        
        // // Only use potion if health was actually restored
        // if (playerStats.currentHealth > previousHealth)
        // {
        //     currentPotions--;
        //     //Update UI
        //     uiManager.currentPotions = currentPotions;
        //     uiManager.UpdatePotionDisplay();
            
        //     // Update heart display in UIManager
        //     uiManager.UpdatePlayerHealthUI();
            
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