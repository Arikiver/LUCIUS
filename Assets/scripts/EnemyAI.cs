using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera; // For line of sight checks
    private NavMeshAgent agent;
    public ThirdPersonCharacter character;

    [Header("Detection Settings")]

    [Tooltip("Yellow color")]
    [SerializeField] private float sightRange = 15f;

    [Tooltip("Red color")]
    [SerializeField] private float soundRange = 10f;

    [Tooltip("Green color")]
    [SerializeField] private float sightAngle = 60f; // Field of view angle

    [SerializeField] private float losePlayerTime = 5f; // Time before losing player

    [Header("Wandering Settings")]

    [Tooltip("Blue color")]
    [SerializeField] private float wanderRadius = 20f;

    [SerializeField] private float wanderTimer = 5f;

    [Header("Player Movement Detection")]
    [SerializeField] private float runningSpeedThreshold = 3f; // Speed threshold to detect running

    // State management
    public enum EnemyState { Wandering, Chasing, Searching }
    private EnemyState currentState = EnemyState.Wandering;

    // Public property to access current state
    public EnemyState CurrentState { get { return currentState; } }

    // Timers and tracking
    private float timer;
    private Vector3 lastKnownPlayerPosition;
    private float timeSincePlayerSeen;
    private Vector3 playerLastPosition;
    private float playerSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        // Start wandering
        SetNewWanderDestination();
        timer = wanderTimer;
    }

    void Update()
    {
        if (player != null)
        {
            // Calculate player speed for sound detection
            playerSpeed = Vector3.Distance(player.position, playerLastPosition) / Time.deltaTime;
            playerLastPosition = player.position;

            // Update state based on detection
            UpdateDetection();

            // Execute behavior based on current state
            switch (currentState)
            {
                case EnemyState.Wandering:
                    WanderBehavior();
                    break;
                case EnemyState.Chasing:
                    ChaseBehavior();
                    break;
                case EnemyState.Searching:
                    SearchBehavior();
                    break;
            }
        }

        // Handle movement animation
        if (agent.remainingDistance > agent.stoppingDistance)
        {
            character.Move(agent.desiredVelocity, false, false);
        }
        else
        {
            character.Move(Vector3.zero, false, false);
        }
    }

    void UpdateDetection()
    {
        bool playerDetected = false;

        // Check if player is in sight
        if (CanSeePlayer())
        {
            playerDetected = true;
            lastKnownPlayerPosition = player.position;
            timeSincePlayerSeen = 0f;
        }

        // Check if player is making noise (running)
        if (CanHearPlayer())
        {
            playerDetected = true;
            lastKnownPlayerPosition = player.position;
            timeSincePlayerSeen = 0f;
        }

        // Update state based on detection
        if (playerDetected && currentState == EnemyState.Wandering)
        {
            Debug.Log("Enemy detected player - switching to chase mode!");
            currentState = EnemyState.Chasing;
        }
        else if (currentState == EnemyState.Chasing || currentState == EnemyState.Searching)
        {
            timeSincePlayerSeen += Time.deltaTime;

            // If we haven't seen the player for too long, go back to wandering
            if (timeSincePlayerSeen > losePlayerTime)
            {
                Debug.Log("Enemy lost player - returning to wander mode");
                currentState = EnemyState.Wandering;
                SetNewWanderDestination();
            }
        }
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is within sight range
        if (distanceToPlayer > sightRange)
            return false;

        // Check if player is within field of view
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > sightAngle / 2)
            return false;

        // Check if there's a clear line of sight (no obstacles)
        Vector3 rayStart = transform.position + Vector3.up * 1.5f; // Eye level
        Vector3 rayEnd = player.position + Vector3.up * 1f; // Player center

        if (Physics.Raycast(rayStart, (rayEnd - rayStart).normalized, out RaycastHit hit, distanceToPlayer))
        {
            // If we hit the player, we can see them
            return hit.transform == player;
        }

        return true; // No obstacles in the way
    }

    bool CanHearPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Player must be within sound range and running fast enough
        return distanceToPlayer <= soundRange && playerSpeed > runningSpeedThreshold;
    }

    void WanderBehavior()
    {
        timer += Time.deltaTime;

        // Check if we've reached our destination or timer expired
        if (timer >= wanderTimer || agent.remainingDistance < 2f)
        {
            SetNewWanderDestination();
            timer = 0f;
        }
    }

    void ChaseBehavior()
    {
        // If we can currently see or hear the player, chase their current position
        if (CanSeePlayer() || CanHearPlayer())
        {
            agent.SetDestination(player.position);
            lastKnownPlayerPosition = player.position;
        }
        else
        {
            // Go to last known position
            agent.SetDestination(lastKnownPlayerPosition);

            // If we reach the last known position, start searching
            if (agent.remainingDistance < 2f)
            {
                currentState = EnemyState.Searching;
                Debug.Log("Enemy reached last known position - starting search");
            }
        }
    }

    void SearchBehavior()
    {
        // Look around the last known position
        if (agent.remainingDistance < 2f)
        {
            timer += Time.deltaTime;
            if (timer >= 2f) // Search for 2 seconds at each point
            {
                // Set a new search point near the last known position
                Vector3 searchPoint = lastKnownPlayerPosition + Random.insideUnitSphere * 5f;
                searchPoint.y = transform.position.y;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(searchPoint, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                timer = 0f;
            }
        }
    }

    void SetNewWanderDestination()
    {
        int attempts = 0;
        while (attempts < 5) // Limit attempts to prevent infinite loops
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
            attempts++;
        }
        // Fallback: stay at current position
        agent.SetDestination(transform.position);
    }


    // Visual debugging in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Draw sound range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, soundRange);

        // Draw field of view
        Gizmos.color = Color.green;
        Vector3 leftBoundary = Quaternion.AngleAxis(-sightAngle / 2, Vector3.up) * transform.forward * sightRange;
        Vector3 rightBoundary = Quaternion.AngleAxis(sightAngle / 2, Vector3.up) * transform.forward * sightRange;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Draw wander radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, wanderRadius);
    }
}