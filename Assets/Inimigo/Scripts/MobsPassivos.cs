// MobsPassivos.cs - VERS�O H�BRIDA (Waypoints ou Aleat�rio)
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MobsPassivos : MonoBehaviour
{
    public enum MobState { Idle, Walking, Fleeing }

    [Header("Refer�ncias Principais")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;

    [Header("Efeitos Visuais")]
    public GameObject bloodEffectPrefab;

    // --- L�GICA DE PATRULHA H�BRIDA ---
    [Header("Configura��es de Patrulha")]
    [Tooltip("Deixe esta lista vazia para o mob andar aleatoriamente.")]
    public Transform[] patrolPoints;
    [Tooltip("Dist�ncia m�xima para encontrar um ponto aleat�rio (usado se n�o houver waypoints).")]
    public float patrolWalkPointRange = 20f;
    public float patrolPauseDuration = 3f;

    // Vari�veis internas para Waypoints
    private int currentPatrolIndex = 0;

    // Vari�veis internas para Patrulha Aleat�ria
    private Vector3 walkPoint;
    private bool walkPointSet;

    // Vari�vel de estado da patrulha
    private bool isWaiting = false;
    // --- FIM DA L�GICA H�BRIDA ---

    [Header("Configura��es de Fuga")]
    public float fleeSpeed = 8f;
    public float fleeDistance = 15f;
    public float fleeDuration = 7f;

    // Vari�veis internas
    private bool isFleeing = false;
    private float normalSpeed;
    private MobState currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        normalSpeed = agent.speed;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("ERRO CR�TICO: O Animator n�o foi associado no Inspector: " + name, this);
            this.enabled = false;
            return;
        }
        ChangeState(MobState.Idle);
    }

    private void Update()
    {
        if (isFleeing) FleeContinuously();
        else Patroling();
    }

    private void ChangeState(MobState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        switch (newState)
        {
            case MobState.Idle: animator.Play("Idle"); break;
            case MobState.Walking: animator.Play("Walking"); break;
            case MobState.Fleeing: animator.Play("Walking"); break;
        }
    }

    private void Patroling()
    {
        if (isWaiting) return;

        if (patrolPoints.Length > 0)
        {
            // MODO WAYPOINT
            ChangeState(MobState.Walking);
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            agent.SetDestination(targetPoint.position);

            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                StartCoroutine(PatrolWaitCoroutine());
            }
        }
        else
        {
            // MODO ALEAT�RIO
            if (!walkPointSet) SearchWalkPoint();
            if (walkPointSet) agent.SetDestination(walkPoint);

            if (walkPointSet && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                StartCoroutine(PatrolWaitCoroutine());
            }
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-patrolWalkPointRange, patrolWalkPointRange);
        float randomX = Random.Range(-patrolWalkPointRange, patrolWalkPointRange);
        Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, patrolWalkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
            agent.SetDestination(walkPoint);
            ChangeState(MobState.Walking);
        }
    }

    private IEnumerator PatrolWaitCoroutine()
    {
        isWaiting = true;
        agent.isStopped = true;
        ChangeState(MobState.Idle);
        yield return new WaitForSeconds(patrolPauseDuration);

        if (patrolPoints.Length > 0)
        {
            GoToNextWaypoint();
        }
        else
        {
            walkPointSet = false;
        }

        isWaiting = false;
        agent.isStopped = false;
    }

    private void GoToNextWaypoint()
    {
        if (patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public void OnHit(Vector3 hitPoint)
    {
        if (bloodEffectPrefab != null) Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
        if (isFleeing) return;
        StartCoroutine(FleeTimerCoroutine());
    }

    private IEnumerator FleeTimerCoroutine()
    {
        isFleeing = true;
        ChangeState(MobState.Fleeing);
        StopAllCoroutines();
        isWaiting = false;
        agent.isStopped = false;
        agent.speed = fleeSpeed;
        yield return new WaitForSeconds(fleeDuration);
        isFleeing = false;
        agent.speed = normalSpeed;
        ChangeState(MobState.Idle);
    }

    private void FleeContinuously()
    {
        if (player == null)
        {
            isFleeing = false;
            return;
        }
        Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
        Vector3 desiredFleePoint = transform.position + directionAwayFromPlayer * fleeDistance;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredFleePoint, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}