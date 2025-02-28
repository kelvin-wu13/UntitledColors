using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetectionRoaming : MonoBehaviour
{
    [SerializeField] public float detectionRange = 5f;
    [SerializeField] private LayerMask playerLayer;
    
    private Transform playerTransform;
    private EnemyPathfinding enemyPathfinding;
    private CrimsonCharger crimsonCharger;
    private Coroutine roamingRoutine;
    private bool isRoaming = false;

    public delegate void PlayerDetectedHandler(Transform player);
    public event PlayerDetectedHandler OnPlayerDetected;

    public delegate void PlayerLostHandler();
    public event PlayerLostHandler OnPlayerLost;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        crimsonCharger = GetComponent<CrimsonCharger>();
    }

    private void Start()
    {
        // Find player
        FindPlayer();
        
        // Start in roaming state
        StartRoaming();
    }

    private void Update()
    {
        // Periodically check for player
        if (isRoaming)
        {
            CheckForPlayerInRange();
        }
        else
        {
            // If we're not roaming, check if player went out of range
            CheckIfPlayerOutOfRange();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    public void StartRoaming()
    {
        if (roamingRoutine != null)
        {
            StopCoroutine(roamingRoutine);
        }
        
        isRoaming = true;
        roamingRoutine = StartCoroutine(RoamingRoutine());
    }

    public void StopRoaming()
    {
        isRoaming = false;
        
        if (roamingRoutine != null)
        {
            StopCoroutine(roamingRoutine);
            roamingRoutine = null;
        }
        
        // Stop movement
        if (enemyPathfinding != null)
        {
            enemyPathfinding.MoveTo(Vector2.zero);
        }
    }

    private IEnumerator RoamingRoutine()
    {
        while (isRoaming)
        {
            Vector2 roamPosition = GetRoamingPosition();
            if (enemyPathfinding != null)
            {
                enemyPathfinding.MoveTo(roamPosition);
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private Vector2 GetRoamingPosition()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
    }

    private void CheckForPlayerInRange()
    {
        if (playerTransform == null)
        {
            // Try to find player again if it wasn't found before
            FindPlayer();
            if (playerTransform == null)
            {
                return; // Still no player found
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer <= detectionRange)
        {
            // Player detected - stop roaming and trigger the event
            StopRoaming();
            OnPlayerDetected?.Invoke(playerTransform);
        }
    }

    private void CheckIfPlayerOutOfRange()
    {
        if (playerTransform == null)
        {
            // If we lost reference to player, start roaming
            OnPlayerLost?.Invoke();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer > detectionRange)
        {
            // Player went out of range
            OnPlayerLost?.Invoke();
        }
    }

    // For debugging - visualize detection range
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}