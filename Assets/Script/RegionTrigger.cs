using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionTrigger : MonoBehaviour
{
    [SerializeField] private string regionName;
    [SerializeField] private Vector2 checkpointPosition;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Check if we have a valid GameManager reference
            if (GameManager.Instance != null)
            {
                // If checkpointPosition is not set, use the trigger's position
                if (checkpointPosition == Vector2.zero)
                {
                    checkpointPosition = transform.position;
                }
                
                // Notify GameManager of region change
                GameManager.Instance.EnterNewRegion(regionName, checkpointPosition);
            }
        }
    }
}