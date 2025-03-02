using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public PlayerStats playerStats;

    [Header("Health")]
    public Image[] healthImages;
    public Sprite fullHealthSprite;
    public Sprite emptyHealthSprite;
    public int maxHealth = 7;
    private int currentHealth;

    [Header("UI Elements")]
    public Image[] potionImages;
    public Sprite fullPotionSprite;
    public int maxPotions = 3;
    public int currentPotions;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Init(PlayerStats playerStats)
    {
        this.playerStats = playerStats;
        if (playerStats != null)
        {
            playerStats.maxHealth = maxHealth;
            playerStats.currentHealth = maxHealth;
        }
        else
        {
            Debug.LogError("PlayerStats not found");
        }

        InitializeHealth();
        UpdatePotionDisplay();
    }

    void InitializeHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthDisplay();
    }

    public void UpdatePlayerHealthUI()
    {
        Debug.Log(playerStats);
        currentHealth = (int)playerStats.currentHealth;
        UpdateHealthDisplay();
    }

    public void UpdateHealthDisplay()
    {
        for (int i = 0; i < healthImages.Length; i++)
        {
            if (i < currentHealth)
            {
                healthImages[i].sprite = fullHealthSprite;
            }
            else
            {
                healthImages[i].sprite = emptyHealthSprite;
            }
        }
        Debug.Log("Complete Update HP UI");
    }

    public void UpdatePotionDisplay()
    {
        // Update potion bottle images
        for (int i = potionImages.Length - 1; i >= 0; i--)
        {
            if (i >= currentPotions)
            {
                if (potionImages[i] != null)
                {
                    Destroy(potionImages[i].gameObject);
                    potionImages[i] = null; // Set the reference to null after destroying
                }
            }
            else
            {
                if (potionImages[i] != null)
                {
                    potionImages[i].sprite = fullPotionSprite;
                }
            }
        }
    }   
}
