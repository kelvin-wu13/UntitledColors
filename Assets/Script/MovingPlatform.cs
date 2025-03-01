// Update MovingPlatform.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool returnToStart = false;
    [SerializeField] private float waitTimeAtDestination = 0f;
    
    // Add symbol marker identification
    [Header("Symbol Settings")]
    [SerializeField] private string symbolTag = "SquareSymbol";
    [SerializeField] private Transform[] waypoints; // Can be set manually or found by tag
    
    [Header("Optional Audio")]
    [SerializeField] private AudioClip movingSound;
    [SerializeField] private AudioClip arrivalSound;
    
    private Vector3 startPosition;
    private Vector3 currentTarget;
    private bool isMoving = false;
    private AudioSource audioSource;
    private int currentWaypointIndex = 0;
    
    private void Awake()
    {
        startPosition = transform.position;
        
        // If waypoints aren't set in inspector, try to find them by tag
        if (waypoints == null || waypoints.Length == 0)
        {
            FindWaypointsByTag();
        }
        
        // Set initial target as the first waypoint (if available)
        if (waypoints.Length > 0)
        {
            targetPosition = waypoints[0].position;
        }
        
        currentTarget = startPosition;
        audioSource = GetComponent<AudioSource>();
    }
    
    private void FindWaypointsByTag()
    {
        // Find all objects with the specified tag
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag(symbolTag);
        waypoints = new Transform[waypointObjects.Length];
        
        for (int i = 0; i < waypointObjects.Length; i++)
        {
            waypoints[i] = waypointObjects[i].transform;
        }
        
        Debug.Log($"Found {waypoints.Length} waypoints with tag '{symbolTag}'");
    }
    
    // This method will be called by the lever through Unity Event
    public void ActivatePlatform()
    {
        if (!isMoving)
        {
            // Move to the next waypoint in sequence
            if (waypoints.Length > 0)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                currentTarget = waypoints[currentWaypointIndex].position;
            }
            // If no waypoints are set, use the original target position logic
            else
            {
                if (currentTarget == startPosition)
                    currentTarget = targetPosition;
                else if (returnToStart)
                    currentTarget = startPosition;
            }
            
            // Start the movement coroutine
            StartCoroutine(MovePlatform());
        }
    }
    
    private IEnumerator MovePlatform()
    {
        isMoving = true;
        
        // Play moving sound if available
        if (audioSource != null && movingSound != null)
        {
            audioSource.clip = movingSound;
            audioSource.Play();
        }
        
        // Move the platform until it reaches the target
        while (Vector3.Distance(transform.position, currentTarget) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Ensure perfect positioning
        transform.position = currentTarget;
        
        // Play arrival sound if available
        if (audioSource != null && arrivalSound != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(arrivalSound);
        }
        
        if (waitTimeAtDestination > 0)
        {
            yield return new WaitForSeconds(waitTimeAtDestination);
        }
        
        isMoving = false;
    }
    
    // Visual representation of the path in the editor
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        
        // Draw lines between waypoints if available
        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            
            // Draw line from current position to first waypoint
            Gizmos.DrawLine(transform.position, waypoints[0].position);
            
            // Draw lines between consecutive waypoints
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                }
            }
            
            // Draw line from last waypoint back to first waypoint to show the cycle
            if (waypoints.Length > 1 && waypoints[0] != null && waypoints[waypoints.Length-1] != null)
            {
                Gizmos.DrawLine(waypoints[waypoints.Length-1].position, waypoints[0].position);
            }
            
            // Draw cubes at each waypoint
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawWireCube(waypoint.position, transform.localScale);
                }
            }
        }
        // If no waypoints, use original target position
        else
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, targetPosition);
            Gizmos.DrawWireCube(targetPosition, transform.localScale);
        }
    }
    
    // Optional: Make platform carry the player
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         collision.transform.SetParent(transform);
    //     }
    // }
    
    // private void OnCollisionExit2D(Collision2D collision)
    // {
    //     if (collision.gameObject.CompareTag("Player"))
    //     {
    //         collision.transform.SetParent(null);
    //     }
    // }
}