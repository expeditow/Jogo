// EnemyAI.cs - VERSÃO HÍBRIDA (Waypoints ou Aleatório)
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Idle, Walking, Attacking }

    [Header("Referências Principais")]
    public NavMeshAgent agent;
    public Transform player;
    public Animator animator;

    [Header("Efeitos Visuais")]
    public GameObject bloodEffectPrefab;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    // --- LÓGICA DE PATRULHA HÍBRIDA ---
    [Header("Configurações de Patrulha")]
    [Tooltip("Deixe esta lista vazia para o inimigo andar aleatoriamente.")]
    public Transform[] patrolPoints;
    [Tooltip("Distância máxima para encontrar um ponto aleatório (usado se não houver waypoints).")]
    public float walkPointRange = 20f;
    public float patrolPauseDuration = 3f;

    // Variáveis internas para Waypoints
    private int currentPatrolIndex = 0;

    // Variáveis internas para Patrulha Aleatória
    private Vector3 walkPoint;
    private bool walkPointSet;

    // Variável de estado da patrulha
    private bool isWaiting = false;
    // --- FIM DA LÓGICA HÍBRIDA ---

    [Header("Configurações de Perseguição e Ataque")]
    public float sightRange = 15f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f;
    private bool alreadyAttacked;

    [Header("Estados (Apenas para Debug Visual)")]
    public bool playerInSightRange;
    public bool playerInAttackRange;
    private EnemyState currentState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else
        {
            Debug.LogError("INIMIGO " + name + ": Não encontrou o jogador na cena!", this);
            this.enabled = false;
        }
    }

    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("ERRO CRÍTICO: O Animator não foi associado no Inspector do inimigo: " + name, this);
            this.enabled = false;
            return;
        }
        ChangeState(EnemyState.Idle);
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        switch (newState)
        {
            case EnemyState.Idle: animator.Play("Idle"); break;
            case EnemyState.Walking: animator.Play("Walking"); break;
            case EnemyState.Attacking: animator.Play("Attack"); break;
        }
    }

    private void Patroling()
    {
        if (isWaiting) return;

        // Verifica se há waypoints definidos
        if (patrolPoints.Length > 0)
        {
            // MODO WAYPOINT
            ChangeState(EnemyState.Walking);
            Transform targetPoint = patrolPoints[currentPatrolIndex];
            agent.SetDestination(targetPoint.position);

            if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                StartCoroutine(PatrolWait());
            }
        }
        else
        {
            // MODO ALEATÓRIO
            if (!walkPointSet) SearchWalkPoint();
            if (walkPointSet) agent.SetDestination(walkPoint);

            if (walkPointSet && agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
            {
                StartCoroutine(PatrolWait());
            }
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);
        Vector3 randomPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, walkPointRange, NavMesh.AllAreas))
        {
            walkPoint = hit.position;
            walkPointSet = true;
            ChangeState(EnemyState.Walking);
        }
    }

    private IEnumerator PatrolWait()
    {
        isWaiting = true;
        agent.isStopped = true;
        ChangeState(EnemyState.Idle);
        yield return new WaitForSeconds(patrolPauseDuration);

        // Decide o que fazer depois da pausa
        if (patrolPoints.Length > 0)
        {
            GoToNextWaypoint();
        }
        else
        {
            walkPointSet = false; // Procura um novo ponto aleatório
        }

        isWaiting = false;
        agent.isStopped = false;
    }

    private void GoToNextWaypoint()
    {
        if (patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void ChasePlayer()
    {
        if (isWaiting)
        {
            isWaiting = false;
            StopAllCoroutines();
        }
        agent.isStopped = false;
        agent.SetDestination(player.position);
        ChangeState(EnemyState.Walking);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        transform.LookAt(player);
        if (!alreadyAttacked)
        {
            ChangeState(EnemyState.Attacking);
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            ChangeState(EnemyState.Idle);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void OnHit(Vector3 hitPoint)
    {
        if (bloodEffectPrefab != null) Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}