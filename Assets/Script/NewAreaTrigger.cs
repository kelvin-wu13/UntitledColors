using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAreaTrigger : MonoBehaviour
{
    public Transform newRespawnPoint;
    public string areaName;
    private PlayerRespawn playerRespawn;

    void Start()
    {
        playerRespawn = FindObjectOfType<PlayerRespawn>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Update the respawn point in PlayerRespawn
            playerRespawn.SetRespawnPoint(newRespawnPoint.position);

            // Notify GameManager of the new respawn point and area
            GameManager.Instance.UpdateRespawnPoint(newRespawnPoint.position, areaName);
        }
    }
}