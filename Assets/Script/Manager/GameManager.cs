using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AreaEnemies
{
    public string areaName;
    public List<CrimsonCharger> enemies = new List<CrimsonCharger>();
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Area Management")]
    [SerializeField] private List<AreaEnemies> areaEnemiesList = new List<AreaEnemies>();
    
    private Vector2 initialRespawnPoint;
    private Vector2 currentRespawnPoint;
    private HashSet<GameObject> defeatedEnemies = new HashSet<GameObject>();
    private HashSet<string> clearedAreas = new HashSet<string>();
    
    // Track checkpoints triggered by the player
    private HashSet<Vector2> triggeredCheckpoints = new HashSet<Vector2>();

    private string currentAreaName = "Savannah";
    private string lastCheckpointAreaName = "Savannah";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Set the initial respawn point to the player's starting position
        initialRespawnPoint = FindObjectOfType<PlayerRespawn>().transform.position;
        currentRespawnPoint = initialRespawnPoint;
        
        // Add initial checkpoint to triggered list
        triggeredCheckpoints.Add(initialRespawnPoint);
        lastCheckpointAreaName = currentAreaName;
        
        // Initialize and validate area enemies
        ValidateAreaEnemies();
    }
    
    private void ValidateAreaEnemies()
    {
        // Make sure all enemies in the lists are valid
        foreach (AreaEnemies areaEnemies in areaEnemiesList)
        {
            // Remove any null entries that might exist
            areaEnemies.enemies.RemoveAll(enemy => enemy == null);
        }
    }

    public void RegisterEnemyDeath(GameObject enemy)
    {
        // Add the defeated enemy to the list
        defeatedEnemies.Add(enemy);
    }

    public void RegisterAreaCleared(string areaName)
    {
        // Mark the area as cleared
        clearedAreas.Add(areaName);
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayerWithDelay());
    }

    private IEnumerator RespawnPlayerWithDelay()
    {
        // Wait for 2.5 seconds before respawning
        yield return new WaitForSeconds(2.5f);
        
        // Find the player respawn component
        PlayerRespawn playerRespawn = FindObjectOfType<PlayerRespawn>();
        
        // Teleport the player to the current respawn point
        playerRespawn.transform.position = currentRespawnPoint;
        
        // Reset player state
        PlayerStats playerStats = playerRespawn.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.RestoreHealth();
        }
        
        // Get player animator and set to idle
        Animator playerAnimator = playerRespawn.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            // Trigger idle animation if you have a specific trigger for it
            playerAnimator.Play("Idle"); // Replace "Idle" with your actual idle animation state name
        }

        // Check if the player has triggered a new checkpoint
        bool hasNewCheckpoint = lastCheckpointAreaName != currentAreaName;
    
        ResetAllEnemiesInArea(currentAreaName);
        Debug.Log(currentAreaName);
    }
    
    private void ResetAllEnemiesInArea(string areaName)
    {
        // Get enemies for the specified area
        AreaEnemies areaEnemies = GetAreaEnemies(areaName);
        if (areaEnemies != null)
        {
            Debug.Log(areaEnemies);
            // Remove any defeated enemies in this area from the defeated list
            foreach (CrimsonCharger enemy in areaEnemies.enemies)
            {
                if (enemy != null)
                {
                    defeatedEnemies.Remove(enemy.gameObject);
                    
                    // Ensure the enemy is active
                    if (!enemy.gameObject.activeSelf)
                    {
                        enemy.gameObject.SetActive(true);
                    }
                    
                    // Reset position, health, and state
                    enemy.gameObject.SetActive(true);
                    enemy.ResetEnemy();
                }
            }
        }
        else
        {
            // Fallback to the original implementation if area not found
            // Clear the entire defeated enemies list
            defeatedEnemies.Clear();
            
            // Find all enemies in the scene
            CrimsonCharger[] allEnemies = FindObjectsOfType<CrimsonCharger>();
            
            foreach (CrimsonCharger enemy in allEnemies)
            {
                // Reset all enemies (restore health and position)
                if (!enemy.gameObject.activeSelf)
                {
                    enemy.gameObject.SetActive(true);
                }
                enemy.ResetEnemy();
            }
        }
    }
    
    private AreaEnemies GetAreaEnemies(string areaName)
    {
        // Find the area in our list
        return areaEnemiesList.Find(area => area.areaName == areaName);
    }
    
    private void ResetAllEnemies()
    {
        // Legacy method - now we use the area-specific version
        ResetAllEnemiesInArea(currentAreaName);
    }

    private void ResetEnemiesInCurrentArea()
    {
        // This method is now replaced by ResetActiveEnemiesOnlyInArea or ResetAllEnemiesInArea
        // depending on checkpoint status
    }

    public void UpdateRespawnPoint(Vector2 newRespawnPoint, string areaName)
    {
        // Only update if this is a new checkpoint
        if (!triggeredCheckpoints.Contains(newRespawnPoint))
        {
            // Store previous checkpoint area before updating
            lastCheckpointAreaName = currentAreaName;
            
            // Update the respawn point
            currentRespawnPoint = newRespawnPoint;
            
            // Add this checkpoint to triggered list
            triggeredCheckpoints.Add(newRespawnPoint);
            
            // Update current area
            currentAreaName = areaName;
            
            // Mark area as cleared if needed
            if (!clearedAreas.Contains(areaName))
            {
                RegisterAreaCleared(areaName);
            }
        }
    }

    public bool IsAreaCleared(string areaName)
    {
        // Check if the area has been cleared
        return clearedAreas.Contains(areaName);
    }
    
    public Vector2 GetCurrentRespawnPoint()
    {
        return currentRespawnPoint;
    }
    
    public string GetCurrentAreaName()
    {
        return currentAreaName;
    }
    
    // Add a new method to manually register an enemy to a specific area
    public void RegisterEnemyToArea(CrimsonCharger enemy, string areaName)
    {
        AreaEnemies areaEnemies = GetAreaEnemies(areaName);
        
        if (areaEnemies == null)
        {
            // Create a new area if it doesn't exist
            areaEnemies = new AreaEnemies { areaName = areaName };
            areaEnemiesList.Add(areaEnemies);
        }
        
        // Add the enemy if it's not already in the list
        if (!areaEnemies.enemies.Contains(enemy))
        {
            areaEnemies.enemies.Add(enemy);
        }
    }
}