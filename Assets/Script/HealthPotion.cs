using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthPotion : MonoBehaviour
{
    [Header("Potion Settings")]
    public int maxPotions = 3;
    public int currentPotions;
    public KeyCode useKey = KeyCode.E;
    
    private PlayerStats playerStats;
    private UIManager uiManager;
    
    void Start()
    {
        playerStats = FindObjectOfType<PlayerStats>();
        uiManager = FindObjectOfType<UIManager>();
        
        if (playerStats == null || uiManager == null)
        {
            Debug.LogError("Required components not found!");
        }
        
        // Initialize potions
        currentPotions = maxPotions;
        uiManager.UpdatePotionDisplay();
    }
    
    void Update()
    {
        // Check for potion use input
        if (Input.GetKeyDown(useKey) && currentPotions > 0)
        {
            UsePotion();
        }
    }
    
    public void UsePotion()
    {
        if (currentPotions <= 0 || playerStats.currentHealth >= playerStats.maxHealth)
            return;
            
        // Heal player
        float previousHealth = playerStats.currentHealth;
        playerStats.currentHealth = playerStats.maxHealth;
        
        // Only use potion if health was actually restored
        if (playerStats.currentHealth > previousHealth)
        {
            currentPotions--;
            uiManager.UpdatePotionDisplay();
            
            // Update heart display in UIManager
            uiManager.UpdatePlayerHealthUI();
            
            // Play effects (optional)
            //PlayPotionEffects();
        }
    }
    
    public void AddPotion()
    {
        if (currentPotions < maxPotions)
        {
            currentPotions++;
            uiManager.UpdatePotionDisplay();
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