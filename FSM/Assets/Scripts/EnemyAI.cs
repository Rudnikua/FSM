using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour {
    public enum Difficulty {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }

    [System.Serializable]
    public class DifficultySettings {

        public float aggroRangeMultiplier = 1f;
        public float attackRangeMultiplier = 1f;
        public float attackCooldownMultiplier = 1f;
        public float searchDurationMultiplier = 1f;
        public float patrolSpeedMultiplier = 1f;
        public float chaseSpeedMultiplier = 1f;

        public static DifficultySettings Medium => new DifficultySettings {
            aggroRangeMultiplier = 1f,
            attackRangeMultiplier = 1f,
            attackCooldownMultiplier = 1f,
            searchDurationMultiplier = 1f,
            patrolSpeedMultiplier = 1f,
            chaseSpeedMultiplier = 1f,
        };

        public static DifficultySettings Easy => new DifficultySettings {
            aggroRangeMultiplier = 0.7f,
            attackRangeMultiplier = 0.8f,
            attackCooldownMultiplier = 1.5f,
            searchDurationMultiplier = 0.5f,
            patrolSpeedMultiplier = 0.8f,
            chaseSpeedMultiplier = 0.8f,
        };

        public static DifficultySettings Hard => new DifficultySettings {
            aggroRangeMultiplier = 1.3f,
            attackRangeMultiplier = 1.1f,
            attackCooldownMultiplier = 0.7f,
            searchDurationMultiplier = 2f,
            patrolSpeedMultiplier = 1.1f,
            chaseSpeedMultiplier = 1.2f,
        };
    }
    private enum State {
        Patrol,
        Chase,
        Attack,
        Search
    }

    private State currentState = State.Patrol;

    [Header("Difficulty")]
    [SerializeField] private Difficulty difficulty = Difficulty.Medium;
    [SerializeField] private bool useCastomValues = false;
    [SerializeField] private DifficultySettings customSettings = new DifficultySettings();

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Detection (Base Values)")]
    [SerializeField] private float baseAggroRange = 7f;
    [SerializeField] private float baseAttackRange = 2f;
    [SerializeField] private float lineOfSightCheckRate = 0.3f;

    [Header("Combat (Base Values)")]
    [SerializeField] private float baseAttackCD = 2f;

    [Header("Patrol")]
    [SerializeField] private float patrolWaitTime = 1f;

    [Header("Patrol & Search (Base Values)")]
    [SerializeField] private float basePatrolSpeed = 3.5f;
    [SerializeField] private float baseChaseSpeed = 4f;
    [SerializeField] private float baseSearchDuration = 3f;

    // A* 
    private Pathfinding pathfinder;
    private List<Vector3> currentPath;
    private int currentPathIndex;
    private float pathUpdateTimer;
    private Vector3 manualVelocity;
    private Vector3 lastPosition;
    //

    // Gravity
    private float verticalVelocity;
    private float gravity = -9.81f;
    private float gravityMultiplier = 2f;
    //

    private DifficultySettings currentSettings;
    private Animator animator;
    private float timeSinceLastAttack;
    private float timeSinceLOS;
    private bool lastLOSResult = false;
    private bool isSearchingInPlace = false;
    private int currentPatrolIndex = 0;
    private float patrolTimer;
    private Vector3 lastSeenPlayerPosition;
    private float searchTimer = 0f;
    private float effectiveAgrroRange;
    private float effectiveAttackRange;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int SearchHash = Animator.StringToHash("Search");

    private void Awake() {
        animator = GetComponent<Animator>();

        pathfinder = FindAnyObjectByType<Pathfinding>();
        if (pathfinder == null) Debug.LogError("Pathfinder is NULL");

        UpdateDifficultySettings();

        if (player != null) {
            lastSeenPlayerPosition = player.position;
        } else {
            lastSeenPlayerPosition = transform.position;
        }
    }

    private void Start() {
        SnapToGround();
        StartPatrol();
    }

    private void Update() {
        manualVelocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        Debug.Log($"{name}: {currentState}");
        //Debug.Log($"Is searching in place: {isSearchingInPlace}");
        Debug.Log(searchTimer);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasLineOfSight();

        UpdateState(distanceToPlayer, hasLOS);

        ExecuteState(distanceToPlayer, hasLOS);
    }

    private void UpdateState(float distanceToPlayer, bool hasLOS) {
        effectiveAgrroRange = baseAggroRange * currentSettings.aggroRangeMultiplier;
        effectiveAttackRange = baseAttackRange * currentSettings.attackRangeMultiplier;

        float giveUpRange = 2f;

        if (hasLOS) {
            lastSeenPlayerPosition = player.position;
        }

        switch (currentState) {
            case State.Patrol:
                if (distanceToPlayer <= effectiveAgrroRange && hasLOS)
                    SetState(State.Chase);
                break;
            case State.Chase:
                if (hasLOS && distanceToPlayer <= effectiveAttackRange)
                    SetState(State.Attack);
                else if (!hasLOS)
                    SetState(State.Search);
                else if (distanceToPlayer > effectiveAgrroRange * giveUpRange)
                    SetState(State.Patrol);
                    break;
            case State.Attack:
                if (!hasLOS && distanceToPlayer <= effectiveAgrroRange)
                    SetState(State.Search);
                else if (distanceToPlayer > effectiveAgrroRange)
                    SetState(State.Chase);
                else if (hasLOS && distanceToPlayer > effectiveAttackRange)
                    SetState(State.Chase);
                break;
            case State.Search:
                if (hasLOS && distanceToPlayer < effectiveAgrroRange * giveUpRange)
                    SetState(State.Chase);
                else if (distanceToPlayer > effectiveAgrroRange * giveUpRange)
                    SetState(State.Patrol);
                    break;
        }
    }


    private void ExecuteState(float distanceToPlayer, bool hasLOS) {
        switch (currentState) {
            case State.Patrol:
                PatrolBehavior();
                break;
            case State.Chase:
                ChaseBehavior();
                break;
            case State.Attack:
                AttackBehavior();
                break;
            case State.Search:
                SearchBehavior();
                break;
        }
    }

    private void SetState(State newState) {
        if (currentState == newState) return;

        OnExitState(currentState);

        currentState = newState;
        //Debug.Log($"{name}: {currentState}");

        OnEnterState(currentState);
    }

    private void OnEnterState(State state) {
        currentPath = null;
        currentPathIndex = 0;

        switch (state) {
            case State.Patrol:
                StartPatrol();
                break;
            case State.Chase:
                break;
            case State.Attack:
                break;
            case State.Search:
                searchTimer = 0f;
                isSearchingInPlace = false;
                RequestPath(lastSeenPlayerPosition);
                break;
        }
    }

    private void OnExitState(State state) {
    }

    private void RequestPath(Vector3 targetPos) {
        if (pathfinder != null) {
            currentPath = pathfinder.FindPath(transform.position, targetPos);
            currentPathIndex = 0;
        }
    }

    private void MoveAlongPath(float speed) {
        if (currentPath == null || currentPathIndex >= currentPath.Count) return;

        Vector3 targetPoint = currentPath[currentPathIndex];

        Vector3 targetPosFlat = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);
        Vector3 newPosition = Vector3.MoveTowards(transform.position, targetPosFlat, speed * Time.deltaTime);

        verticalVelocity += gravity * gravityMultiplier * Time.deltaTime;

        float rayStartHeight = 2.0f; 
        float rayLength = 5.0f;      

        Vector3 rayOrigin = newPosition + Vector3.up * rayStartHeight;
        RaycastHit hit;

        Debug.DrawRay(rayOrigin, Vector3.down * rayLength, Color.magenta);

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayLength, groundLayer)) {
            float groundHeight = hit.point.y;

            float potentialY = newPosition.y + (verticalVelocity * Time.deltaTime);

            if (potentialY <= groundHeight) {
                newPosition.y = groundHeight; 
                if (verticalVelocity < 0) {
                    verticalVelocity = -2f;
                }
            } else {
                newPosition.y = potentialY;
            }
        } else {
            newPosition.y += verticalVelocity * Time.deltaTime;
        }

        transform.position = newPosition;

        Vector3 direction = (targetPosFlat - transform.position).normalized;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f) {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            float rotationSpeed = 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

        float distanceToTarget = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(targetPoint.x, targetPoint.z)
        );

        if (distanceToTarget < 0.2f) {
            currentPathIndex++;
        }
    }

    private void PatrolBehavior() {
        if (currentPath == null || currentPathIndex >= currentPath.Count) {
            if (patrolTimer <= 0) {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

                RequestPath(patrolPoints[currentPatrolIndex].position);

                patrolTimer = patrolWaitTime;
            } else {
                patrolTimer -= Time.deltaTime;
            }
        } else {
            MoveAlongPath(basePatrolSpeed * currentSettings.patrolSpeedMultiplier);
        }

            UpdateAnimatorSpeed(basePatrolSpeed);
    }

    private void ChaseBehavior() {
        pathUpdateTimer += Time.deltaTime;
        if (pathUpdateTimer > 0.2f) {
            RequestPath(player.position);
            pathUpdateTimer = 0f;
        }

        MoveAlongPath(baseChaseSpeed * currentSettings.chaseSpeedMultiplier);
        UpdateAnimatorSpeed(baseChaseSpeed);
    }

    private void AttackBehavior() {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }

        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack >= baseAttackCD * currentSettings.attackCooldownMultiplier) {
            animator.SetTrigger(AttackHash);
            timeSinceLastAttack = 0;
        }

        UpdateAnimatorSpeed(baseChaseSpeed);
    }

    private void SearchBehavior() {

        if (!isSearchingInPlace) {
            MoveAlongPath(baseChaseSpeed * currentSettings.chaseSpeedMultiplier);
            UpdateAnimatorSpeed(baseChaseSpeed);

            if (currentPath == null || currentPathIndex >= currentPath.Count) {
                if (Vector3.Distance(transform.position, lastSeenPlayerPosition) < 1.0f) {
                    isSearchingInPlace = true;
                    searchTimer = 0f;
                    animator.SetTrigger(SearchHash);
                }
            }
        } else {
            searchTimer += Time.deltaTime;
            float effectiveSearchDuration = baseSearchDuration * currentSettings.searchDurationMultiplier;

            if (searchTimer >= effectiveSearchDuration) {
                SetState(State.Patrol);
            }
        }
    }
    private void StartPatrol() {
        if (patrolPoints.Length == 0) {
            Debug.LogWarning("No patrol points assigned!");
            return;
        }

        currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        RequestPath(patrolPoints[currentPatrolIndex].position);
        patrolTimer = patrolWaitTime;
    }
    private void UpdateDifficultySettings() {
        if (useCastomValues) {
            currentSettings = customSettings;
        } else {
            switch (difficulty) {
                case Difficulty.Easy:
                    currentSettings = DifficultySettings.Easy;
                    break;
                case Difficulty.Hard:
                    currentSettings = DifficultySettings.Hard;
                    break;
                default: 
                    currentSettings = DifficultySettings.Medium;
                    break;
            }
        }
    }
    private void UpdateAnimatorSpeed(float maxSpeed) {
        float speed = manualVelocity.magnitude;
        float normalizedSpeed = speed / maxSpeed;

        if (normalizedSpeed < 0.05f) normalizedSpeed = 0f;

        float dampTime = 0.1f;
        animator.SetFloat(SpeedHash, normalizedSpeed, dampTime, Time.deltaTime);
    }

    private bool HasLineOfSight() {
        timeSinceLOS += Time.deltaTime;
        if (timeSinceLOS < lineOfSightCheckRate) return lastLOSResult;

        timeSinceLOS = 0;

        Vector3 origin = transform.position + transform.forward * 0.4f + Vector3.up * 1.4f;
        Vector3 target = player.position + Vector3.up * 1.6f;
        Vector3 direction = target - origin;
        float distance = direction.magnitude;

        Debug.DrawRay(origin, direction, lastLOSResult ? Color.green : Color.red, lineOfSightCheckRate);

        bool hitPlayer = Physics.Raycast(origin, direction, out RaycastHit hit, distance);
        if (hitPlayer && hit.transform.CompareTag("Player")) {
            lastLOSResult = true;
        } else lastLOSResult = false;
        //Debug.Log(lastLOSResult);
        return lastLOSResult;
    }
    private void SnapToGround() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hit, 20f, groundLayer)) {
            transform.position = hit.point;
            verticalVelocity = 0;
        }
    }

    private void OnDrawGizmosSelected() {
        if (patrolPoints != null) {
            Gizmos.color = Color.cyan;
            foreach (Transform patrolPoint in patrolPoints) {
                if (patrolPoint != null) Gizmos.DrawSphere(patrolPoint.position, 0.3f);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectiveAgrroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, effectiveAttackRange);

        if (currentPath != null) {
            Gizmos.color = Color.blue;
            for (int i = currentPathIndex; i < currentPath.Count; i++) {
                //Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }
        }
    }
}