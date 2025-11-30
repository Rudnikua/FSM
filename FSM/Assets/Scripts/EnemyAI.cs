using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour {
    private enum State {
        Patrol,
        Chase,
        Attack,
        Search
    }

    private State currentState = State.Patrol;

    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Detection")]
    [SerializeField] private float aggroRange = 7f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float lineOfSightCheckRate = 0.3f;

    [Header("Combat")]
    [SerializeField] private float attackCD = 2f;

    [Header("Patrol")]
    [SerializeField] private float patrolWaitTime = 1f;
    [SerializeField] private float patrolPointRadius = 0.5f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timeSinceLastAttack;
    private float timeSinceLOS;
    private bool lastLOSResult = false;
    private bool isSearchingInPlace = false;
    private int currentPatrolIndex = 0;
    private float patrolTimer;
    private Vector3 lastSeenPlayerPosition;
    private float searchTimer = 0f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int SearchHash = Animator.StringToHash("Search");

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = true;
        agent.angularSpeed = 500f;

        if (player != null) {
            lastSeenPlayerPosition = player.position;
        } else {
            lastSeenPlayerPosition = transform.position;
        }
    }

    private void Start() {
        StartPatrol();
    }

    private void Update() {
        Debug.Log($"{name}: {currentState}");
        Debug.Log($"Is searching in place: {isSearchingInPlace}");
        Debug.Log(searchTimer);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLOS = HasLineOfSight();

        UpdateState(distanceToPlayer, hasLOS);

        ExecuteState(distanceToPlayer, hasLOS);
    }

    private void UpdateState(float distanceToPlayer, bool hasLOS) {
        if (hasLOS) {
            lastSeenPlayerPosition = player.position;
        }

        switch (currentState) {
            case State.Patrol:
                if (distanceToPlayer <= aggroRange && hasLOS)
                    SetState(State.Chase);
                break;
            case State.Chase:
                if (!hasLOS && distanceToPlayer <= aggroRange)
                    SetState(State.Search);
                else if (distanceToPlayer > aggroRange)
                    SetState(State.Search);
                else if (distanceToPlayer <= attackRange && hasLOS)
                    SetState(State.Attack);
                break;
            case State.Attack:
                if (!hasLOS && distanceToPlayer <= aggroRange)
                    SetState(State.Search);
                else if (distanceToPlayer > aggroRange)
                    SetState(State.Search);
                else if (distanceToPlayer > attackRange && hasLOS)
                    SetState(State.Chase);
                break;
            case State.Search:
                if (hasLOS)
                    SetState(State.Chase);
                else if (distanceToPlayer > aggroRange * 2f)
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
        switch (state) {
            case State.Patrol:
                agent.isStopped = false;
                StartPatrol();
                break;
            case State.Chase:
                agent.isStopped = false;
                break;
            case State.Attack:
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
                break;
            case State.Search:
                agent.isStopped = false;
                searchTimer = 0f;
                isSearchingInPlace = false;
                agent.SetDestination(lastSeenPlayerPosition);
                break;
        }
    }

    private void OnExitState(State state) {
    }

    private void PatrolBehavior() {
        if (agent.remainingDistance <= patrolPointRadius && !agent.pathPending) {
            if (patrolTimer <= 0) {
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                patrolTimer = patrolWaitTime;
            } else {
                patrolTimer -= Time.deltaTime;
            }
        }

        UpdateAnimatorSpeed();
    }

    private void ChaseBehavior() {
        agent.SetDestination(player.position);
        UpdateAnimatorSpeed();
    }

    private void AttackBehavior() {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
        }

        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack >= attackCD) {
            animator.SetTrigger(AttackHash);
            timeSinceLastAttack = 0;
        }
    }

    private void SearchBehavior() {

        if (!isSearchingInPlace) {
            agent.SetDestination(lastSeenPlayerPosition);
            UpdateAnimatorSpeed();

            if (!agent.pathPending && agent.remainingDistance <= 0.5f) {
                agent.isStopped = true;
                isSearchingInPlace = true;
                searchTimer = 0f;
                animator.SetTrigger(SearchHash);
            }
        } else {
            searchTimer += Time.deltaTime;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (searchTimer >= searchDuration) {
                SetState(State.Patrol);
            }
        }
    }
    private void StartPatrol() {
        if (patrolPoints.Length == 0) {
            Debug.LogWarning("No patrol points assigned!");
            agent.isStopped = true;
            return;
        }

        currentPatrolIndex = Random.Range(0, patrolPoints.Length);
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        patrolTimer = patrolWaitTime;
    }
    private void UpdateAnimatorSpeed() {
        float speed = agent.velocity.magnitude;
        float normilizedspeed = speed / agent.speed;
        if (normilizedspeed < 0.05f) normilizedspeed = 0f;
        animator.SetFloat(SpeedHash, normilizedspeed);
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
        Debug.Log(lastLOSResult);
        return lastLOSResult;
    }

    private void OnDrawGizmosSelected() {
        if (patrolPoints != null) {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++) {
                if (patrolPoints[i] != null) {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                    if (i < patrolPoints.Length - 1) {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    } else if (patrolPoints.Length > 1) {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}