using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Vector2 initialRespawnPoint;
    private Vector2 currentRespawnPoint;
    private HashSet<GameObject> defeatedEnemies = new HashSet<GameObject>();
    private HashSet<string> clearedAreas = new HashSet<string>();

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
        // Respawn the player at the current checkpoint
        PlayerRespawn playerRespawn = FindObjectOfType<PlayerRespawn>();
        playerRespawn.RespawnPlayer();

        // Respawn enemies in the current area
        RespawnEnemiesInCurrentArea();
    }

    private void RespawnEnemiesInCurrentArea()
    {
        // Find all enemies in the current area
        CrimsonCharger[] enemies = FindObjectsOfType<CrimsonCharger>();

        foreach (CrimsonCharger enemy in enemies)
        {
            // If the enemy was not previously defeated, respawn it
            if (!defeatedEnemies.Contains(enemy.gameObject))
            {
                enemy.ResetEnemy();
            }
        }
    }

    public void UpdateRespawnPoint(Vector2 newRespawnPoint, string areaName)
    {
        // Update the respawn point and mark the area as cleared
        currentRespawnPoint = newRespawnPoint;
        RegisterAreaCleared(areaName);
    }

    public bool IsAreaCleared(string areaName)
    {
        // Check if the area has been cleared
        return clearedAreas.Contains(areaName);
    }
}