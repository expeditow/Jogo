// InimigoNormal.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class InimigoNormal : MonoBehaviour
{
    // NOVO: Enum para os estados de anima��o, deixando o c�digo mais leg�vel.
    public enum EnemyState { Idle, Walking, Attacking }

    [Header("Refer�ncias Principais")]
    public NavMeshAgent agent;
    public Transform player;
    [Tooltip("Arraste o componente Animator do inimigo para este campo no Inspector.")]
    public Animator animator; // NOVO: Refer�ncia para o Animator.

    [Header("Efeitos Visuais")]
    [Tooltip("O prefab do efeito que ser� criado quando o inimigo for atingido.")]
    public GameObject bloodEffectPrefab;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    [Header("Configura��es de Patrulha por Waypoints")]
    public Transform[] patrolPoints;
    public float patrolPauseDuration = 3f;
    private int currentPatrolIndex = 0;
    private bool isWaiting = false;

    [Header("Configura��es de Persegui��o e Ataque")]
    public float sightRange = 15f;
    public float attackRange = 2f;
    public int attackDamage = 10;
    public float timeBetweenAttacks = 2f;
    private bool alreadyAttacked;

    [Header("Estados (Apenas para Debug Visual)")]
    public bool playerInSightRange;
    public bool playerInAttackRange;
    private EnemyState currentState; // NOVO: Vari�vel para guardar o estado da anima��o.

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
            Debug.LogError("INIMIGO " + name + ": N�o encontrou o jogador na cena!", this);
            this.enabled = false;
        }
    }

    // NOVO: M�todo Start para verifica��es iniciais e estado inicial.
    private void Start()
    {
        if (animator == null)
        {
            Debug.LogError("ERRO CR�TICO: O Animator n�o foi associado no Inspector do inimigo: " + name, this);
            this.enabled = false;
            return;
        }
        // Garante que o inimigo comece na anima��o de parado.
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

    // NOVO: M�todo central para controlar as anima��es.
    private void ChangeState(EnemyState newState)
    {
        // Otimiza��o: evita tocar a mesma anima��o repetidamente.
        if (currentState == newState) return;

        currentState = newState;
        
        // O nome do estado no Play() deve ser ID�NTICO ao nome do estado no seu Animator.
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

        // ANIMA��O: Se n�o est� esperando, est� andando.
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
        ChangeState(EnemyState.Idle); // ANIMA��O: Fica parado durante a pausa.

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
        ChangeState(EnemyState.Walking); // ANIMA��O: Perseguir usa a anima��o de andar.
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;
        Vector3 positionToLookAt = player.position;
        positionToLookAt.y = transform.position.y;
        transform.LookAt(positionToLookAt);

        if (!alreadyAttacked)
        {
            ChangeState(EnemyState.Attacking); // ANIMA��O: Toca a anima��o de ataque.
            
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
            // ANIMA��O: Enquanto espera o cooldown do ataque, fica na anima��o de parado.
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