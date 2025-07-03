// InimigoNormal.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class InimigoNormal : MonoBehaviour
{
    // NOVO: Enum para os estados de animação, deixando o código mais legível.
    public enum EnemyState { Idle, Walking, Attacking }

    [Header("Referências Principais")]
    public NavMeshAgent agent;
    public Transform player;
    [Tooltip("Arraste o componente Animator do inimigo para este campo no Inspector.")]
    public Animator animator; // NOVO: Referência para o Animator.

    [Header("Efeitos Visuais")]
    [Tooltip("O prefab do efeito que será criado quando o inimigo for atingido.")]
    public GameObject bloodEffectPrefab;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [Header("Configurações de Patrulha por Waypoints")]
    public Transform[] patrolPoints;
    public float patrolPauseDuration = 3f;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;

    [Header("Configurações de Perseguição e Ataque")]
    public float sightRange = 15f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f;
    private bool alreadyAttacked;

    [Header("Estados (Apenas para Debug Visual)")]
    public bool playerInSightRange;
    public bool playerInAttackRange;
    private EnemyState currentState; // NOVO: Variável para guardar o estado da animação.

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("INIMIGO " + name + ": Não encontrou o jogador na cena!", this);
            this.enabled = false;
        }
    }

    // NOVO: Método Start para verificações iniciais e estado inicial.
    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("ERRO CRÍTICO: O Animator não foi associado no Inspector do inimigo: " + name, this);
            this.enabled = false;
            return;
        }
        // Garante que o inimigo comece na animação de parado.
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

    // NOVO: Método central para controlar as animações.
    private void ChangeState(EnemyState newState)
    {
        // Otimização: evita tocar a mesma animação repetidamente.
        if (currentState == newState) return;

        currentState = newState;
        
        // O nome do estado no Play() deve ser IDÊNTICO ao nome do estado no seu Animator.
        switch (newState)
        {
            case EnemyState.Idle:
                animator.Play("Idle"); 
                break;
            case EnemyState.Walking:
                animator.Play("Walking");
                break;
            case EnemyState.Attacking:
                animator.Play("Attack");
                break;
        }
    }

    private void Patroling()
    {
        if (patrolPoints.Length == 0 || isWaiting)
        {
            return;
        }

        // ANIMAÇÃO: Se não está esperando, está andando.
        ChangeState(EnemyState.Walking);

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        agent.SetDestination(targetPoint.position);

        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StartCoroutine(PatrolWait());
        }
    }

    private IEnumerator PatrolWait()
    {
        isWaiting = true;
        agent.isStopped = true;
        ChangeState(EnemyState.Idle); // ANIMAÇÃO: Fica parado durante a pausa.

        yield return new WaitForSeconds(patrolPauseDuration);

        GoToNextWaypoint();

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
        ChangeState(EnemyState.Walking); // ANIMAÇÃO: Perseguir usa a animação de andar.
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        Vector3 positionToLookAt = player.position;
        positionToLookAt.y = transform.position.y;
        transform.LookAt(positionToLookAt);

        if (!alreadyAttacked)
        {
            ChangeState(EnemyState.Attacking); // ANIMAÇÃO: Toca a animação de ataque.
            
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
        else
        {
            // ANIMAÇÃO: Enquanto espera o cooldown do ataque, fica na animação de parado.
            ChangeState(EnemyState.Idle);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    public void OnHit(Vector3 hitPoint)
    {
        if (bloodEffectPrefab != null)
        {
            Instantiate(bloodEffectPrefab, hitPoint, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}