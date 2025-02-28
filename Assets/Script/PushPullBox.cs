using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushPullBox : MonoBehaviour
{
    public float interactionDistance = 1f;
    public LayerMask obstacleLayer;
    public LayerMask boxLayer;

    private GameObject box;
    private bool isPushing = false;
    private Vector2 pushDirection;
    private Vector2 movementAxis;
    public Vector2 facingDirection = Vector2.right;
    private Vector2 initialInteractionDirection;

    private Transform playerTransform;
    private PlayerController playerController;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerController = playerTransform.GetComponent<PlayerController>();
    }

    void Update()
    {
        HandleInput();
        MoveBox();
        UpdateFacingDirection();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPushing)
            {
                RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, facingDirection, interactionDistance, boxLayer);
                if (hit.collider != null && hit.collider.CompareTag("Box"))
                {
                    box = hit.collider.gameObject;
                    isPushing = true;
                    pushDirection = (box.transform.position - playerTransform.position).normalized;
                    initialInteractionDirection = pushDirection;

                    // Set movement restriction on the player controller
                    if (playerController != null)
                    {
                        playerController.SetMovementRestriction(Mathf.Abs(pushDirection.x) > Mathf.Abs(pushDirection.y));
                    }

                    if (Mathf.Abs(pushDirection.x) > Mathf.Abs(pushDirection.y))
                    {
                        movementAxis = Vector2.right;
                    }
                    else
                    {
                        movementAxis = Vector2.up;
                    }
                }
            }
            else
            {
                // Remove movement restriction when stopping interaction
                if (playerController != null)
                {
                    playerController.ClearMovementRestriction();
                }
                isPushing = false;
                box = null;
            }
        }
    }

    void MoveBox()
    {
        if (isPushing && box != null)
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector2 movement = RestrictMovement(horizontal, vertical);

            if (movement.magnitude > 0)
            {
                Vector2 newPosition = (Vector2)box.transform.position + movement * Time.deltaTime;

                if (!Physics2D.OverlapCircle(newPosition, 0.1f, obstacleLayer))
                {
                    box.transform.position = newPosition;
                    playerTransform.position = (Vector2)box.transform.position - initialInteractionDirection * interactionDistance;
                }
            }
        }
    }

    Vector2 RestrictMovement(float horizontal, float vertical)
    {
        Vector2 movement = Vector2.zero;

        if (Mathf.Abs(initialInteractionDirection.x) > Mathf.Abs(initialInteractionDirection.y))
        {
            movement = new Vector2(horizontal, 0).normalized;
        }
        else
        {
            movement = new Vector2(0, vertical).normalized;
        }

        return movement;
    }

    void UpdateFacingDirection()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            facingDirection = new Vector2(horizontal, vertical).normalized;
        }
    }

    private void OnDrawGizmos()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerTransform.position, playerTransform.position + (Vector3)facingDirection * interactionDistance);
        }
    }
}