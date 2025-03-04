using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveEntranceTeleporter : MonoBehaviour
{
    [Tooltip("The target location where the player will be teleported")]
    public Transform targetLocation;
    
    [Tooltip("Optional delay before teleporting (in seconds)")]
    public float teleportDelay = 0.0f;
    
    [Tooltip("Optional fade effect duration (in seconds)")]
    public float fadeDuration = 0.0f;
    
    [Tooltip("Optional sound effect to play when teleporting")]
    public AudioClip teleportSound;
    
    private AudioSource audioSource;
    
    private void Start()
    {
        // Add an audio source component if a teleport sound is assigned
        if (teleportSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = teleportSound;
            audioSource.playOnAwake = false;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object entering the trigger is the player
        if (collision.CompareTag("Player"))
        {
            TeleportPlayer(collision.gameObject);
        }
    }
    
    // Alternatively, use OnTriggerEnter for 3D colliders
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player
        if (other.CompareTag("Player"))
        {
            TeleportPlayer(other.gameObject);
        }
    }
    
    private void TeleportPlayer(GameObject player)
    {
        // Check if target location is assigned
        if (targetLocation == null)
        {
            Debug.LogError("Target location not assigned to teleporter!");
            return;
        }
        
        // Play sound effect if assigned
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        // If there's a delay, use coroutine, otherwise teleport immediately
        if (teleportDelay > 0 || fadeDuration > 0)
        {
            StartCoroutine(DelayedTeleport(player));
        }
        else
        {
            // Immediately teleport the player to the target location
            player.transform.position = targetLocation.position;
        }
    }
    
    private IEnumerator DelayedTeleport(GameObject player)
    {
        // If using fade effect
        if (fadeDuration > 0)
        {
            // You would implement fade out effect here
            // This is just a placeholder - you'll need to add your own fade system
            Debug.Log("Fading out...");
        }
        
        // Wait for the specified delay
        yield return new WaitForSeconds(teleportDelay);
        
        // Teleport the player
        player.transform.position = targetLocation.position;
        
        // If using fade effect
        if (fadeDuration > 0)
        {
            // You would implement fade in effect here
            Debug.Log("Fading in...");
        }
    }
}