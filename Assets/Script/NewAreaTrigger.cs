using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewAreaTrigger : MonoBehaviour
{
    public Transform newRespawnPoint;
    private PlayerRespawn playerRespawn;

    void Start()
    {
        playerRespawn = FindObjectOfType<PlayerRespawn>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRespawn.SetRespawnPoint(newRespawnPoint.position);
        }
    }
}