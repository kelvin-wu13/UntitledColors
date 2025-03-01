using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerRespawn playerRespawn;
    [SerializeField] private PlayerStats playerStats;

    [SerializeField] private LayerMask enemyLayer;
    
    // List to track enemies in the current region
    private List<EnemyData> currentRegionEnemies = new List<EnemyData>();
    
    // Dictionary to track defeated enemies by region
    private Dictionary<string, List<GameObject>> defeatedEnemiesByRegion = new Dictionary<string, List<GameObject>>();
    
    // Track player's current region
    private string currentRegion = "Starting";

    // Enemy prefab reference
    [SerializeField] private GameObject crimsonChargerPrefab;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Find references if not assigned
        if (playerRespawn == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerRespawn = player.GetComponent<PlayerRespawn>();
                playerStats = player.GetComponent<PlayerStats>();
            }
        }

        // Initialize the first region's list
        defeatedEnemiesByRegion[currentRegion] = new List<GameObject>();
        
        // Subscribe to player death event
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath += HandlePlayerDeath;
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerStats != null)
        {
            playerStats.OnPlayerDeath -= HandlePlayerDeath;
        }
    }

    public void SetCurrentRegion(string regionName)
    {
        // Only process if we're actually changing regions
        if (regionName != currentRegion)
        {
            // Store the old region
            string oldRegion = currentRegion;
            
            // Update current region
            currentRegion = regionName;
            
            // Create a new list for this region if it doesn't exist
            if (!defeatedEnemiesByRegion.ContainsKey(currentRegion))
            {
                defeatedEnemiesByRegion[currentRegion] = new List<GameObject>();
            }
            
            // Update the checkpoint when entering a new region
            if (playerRespawn != null)
            {
                playerRespawn.UpdateRespawnPoint();
            }
            
            // Clear defeated enemies from previous regions
            foreach (var region in defeatedEnemiesByRegion.Keys)
            {
                if (region != currentRegion)
                {
                    foreach (var enemy in defeatedEnemiesByRegion[region])
                    {
                        if (enemy != null)
                        {
                            Destroy(enemy);
                        }
                    }
                }
            }
            
            Debug.Log($"Region changed from {oldRegion} to {currentRegion}");
        }
    }

    public void RegisterEnemy(GameObject enemy)
    {
        // Add enemy to current region tracking
        if (!defeatedEnemiesByRegion[currentRegion].Contains(enemy))
        {
            defeatedEnemiesByRegion[currentRegion].Add(enemy);
        }
    }

    public void RegisterEnemyDeath(GameObject enemy)
    {
        // When an enemy dies, just mark it as inactive but keep tracking it
        if (defeatedEnemiesByRegion[currentRegion].Contains(enemy))
        {
            // We keep the reference so we can respawn it if needed
        }
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        // Wait a short time before respawning
        yield return new WaitForSeconds(2f);
        
        // Respawn player at checkpoint
        if (playerRespawn != null)
        {
            playerRespawn.RespawnPlayer();
        }
        
        // Restore player's health
        if (playerStats != null)
        {
            playerStats.RestoreHealth();
        }
        
        // Respawn enemies in the current region
        RespawnEnemiesInCurrentRegion();
    }

    private void RespawnEnemiesInCurrentRegion()
    {
        if (defeatedEnemiesByRegion.ContainsKey(currentRegion))
        {
            // Create a temporary list to iterate through
            List<GameObject> enemies = new List<GameObject>(defeatedEnemiesByRegion[currentRegion]);
            
            foreach (GameObject enemy in enemies)
            {
                // If the enemy reference is null, it was destroyed
                if (enemy == null)
                {
                    // Create a new enemy at the same position
                    Vector3 respawnPosition = GetValidRespawnPosition();
                    GameObject newEnemy = Instantiate(crimsonChargerPrefab, respawnPosition, Quaternion.identity);
                    
                    // Replace the old reference with the new one
                    defeatedEnemiesByRegion[currentRegion].Remove(enemy);
                    defeatedEnemiesByRegion[currentRegion].Add(newEnemy);
                }
                else if (!enemy.activeInHierarchy)
                {
                    // If the enemy is just inactive, reactivate it
                    enemy.SetActive(true);
                    
                    // Reset enemy state (assuming enemies have a ResetEnemy method)
                    CrimsonCharger charger = enemy.GetComponent<CrimsonCharger>();
                    if (charger != null)
                    {
                        // You would need to add this method to the CrimsonCharger class
                        // charger.ResetEnemy();
                    }
                }
            }
        }
    }

    private Vector3 GetValidRespawnPosition()
    {
        // Simple method to get a spawn position within the current region
        // This would need to be improved based on your specific game layout
        
        // Get player's respawn point as reference
        Vector3 basePosition = playerRespawn.transform.position;
        
        // Random offset within a radius
        float randomAngle = Random.Range(0f, 2f * Mathf.PI);
        float randomDistance = Random.Range(5f, 15f);
        
        Vector3 offset = new Vector3(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance,
            0
        );
        
        return basePosition + offset;
    }

    // Public method for region triggers to call
    public void EnterNewRegion(string regionName, Vector2 checkpointPosition)
    {
        // Set new region
        SetCurrentRegion(regionName);
        
        // Update checkpoint
        if (playerRespawn != null)
        {
            playerRespawn.SetRespawnPoint(checkpointPosition);
        }
    }

    // For scene loading/reloading
    public void RestartGame()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    // Public method for enemies to register themselves
    public void RegisterEnemyInCurrentRegion(GameObject enemy)
    {
        if (!defeatedEnemiesByRegion[currentRegion].Contains(enemy))
        {
            defeatedEnemiesByRegion[currentRegion].Add(enemy);
        }
    }
}