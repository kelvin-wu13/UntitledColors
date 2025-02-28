using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableBlock : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float interactionDistance = 1.5f;
    
    private bool isBeingMoved = false;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 interactionDirection;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.drag = 10f;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public bool CanInteract(Vector2 playerPosition, Vector2 interactDirection)
    {
        Vector2 directionToBlock = ((Vector2)transform.position - playerPosition).normalized;
        float dot = Vector2.Dot(interactDirection, directionToBlock);
        float distance = Vector2.Distance(transform.position, playerPosition);
        
        return distance <= interactionDistance && Mathf.Abs(dot) > 0.9f;
    }

    public void StartInteraction(Transform player, Vector2 direction)
    {
        isBeingMoved = true;
        playerTransform = player;
        interactionDirection = direction;
        rb.isKinematic = false;
    }

    public void StopInteraction()
    {
        isBeingMoved = false;
        playerTransform = null;
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        // Snap to grid
        Vector3 pos = transform.position;
        transform.position = new Vector3(
            Mathf.Round(pos.x * 2) / 2,
            Mathf.Round(pos.y * 2) / 2,
            pos.z
        );
    }

    public void UpdateMovement(Vector2 movement)
    {
        if (!isBeingMoved) return;
        rb.velocity = movement;
    }

    public bool IsBeingMoved()
    {
        return isBeingMoved;
    }

    public Vector2 GetInteractionDirection()
    {
        return interactionDirection;
    }
}