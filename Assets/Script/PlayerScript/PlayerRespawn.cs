using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    private Vector2 respawnPoint;
    private string currentSceneName;

    void Start()
    {
        // Record the initial respawn point as the player's starting position
        respawnPoint = transform.position;
        currentSceneName = SceneManager.GetActiveScene().name;

        // Notify GameManager of the initial respawn point
        GameManager.Instance.UpdateRespawnPoint(respawnPoint, "Savannah");
    }

    void Update()
    {
        // Check if the player has changed scenes
        if (SceneManager.GetActiveScene().name != currentSceneName)
        {
            UpdateRespawnPoint();
            currentSceneName = SceneManager.GetActiveScene().name;
        }
    }

    public void RespawnPlayer()
    {
        // Move the player to the respawn point
        GameManager.Instance.RespawnPlayer();
    }

    public void UpdateRespawnPoint()
    {
        // Update the respawn point to the player's current position
        respawnPoint = transform.position;
        GameManager.Instance.UpdateRespawnPoint(respawnPoint, SceneManager.GetActiveScene().name);
        Debug.Log("RespawnPoint Updated");
    }

    public void SetRespawnPoint(Vector2 newRespawnPoint)
    {
        // Manually set a new respawn point (e.g., when entering a new area)
        respawnPoint = newRespawnPoint;
        GameManager.Instance.UpdateRespawnPoint(respawnPoint, SceneManager.GetActiveScene().name);
    }
}