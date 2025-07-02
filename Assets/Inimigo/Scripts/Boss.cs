using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class InimigoNormal : MonoBehaviour
{
    [Header("Referências Principais")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Layers")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    // --- VARIÁVEIS DE PATRULHA POR WAYPOINTS ---
    [Header("Configurações de Patrulha por Waypoints")]
    public Transform[] patrolPoints; // Lista de pontos para patrulhar
    public float patrolPauseDuration = 3f; // Pausa em cada ponto
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

    private void Awake()
    {
        // ... (o método Awake continua o mesmo)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
        else
        {
            Debug.LogError("INIMIGO " + name + ": Não encontrou o jogador na cena! Verifique se o jogador tem a tag 'Player'.", this);
            this.enabled = false;
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("INIMIGO " + name + ": Componente NavMeshAgent não encontrado!", this);
            this.enabled = false;
        }
    }

    private void Update()
    {
        // ... (o método Update continua o mesmo)
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        else if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        else if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    // --- LÓGICA DE PATRULHA REESCRITA PARA WAYPOINTS ---
    private void Patroling()
    {
        // Se não houver pontos de patrulha definidos, não faz nada.
        if (patrolPoints.Length == 0 || isWaiting)
        {
            return;
        }

        // Define o destino para o ponto de patrulha atual.
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        agent.SetDestination(targetPoint.position);

        // Se chegou ao destino, inicia a pausa.
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            StartCoroutine(PatrolWait());
        }
    }

    private IEnumerator PatrolWait()
    {
        isWaiting = true;
        agent.isStopped = true;
        yield return new WaitForSeconds(patrolPauseDuration);

        GoToNextWaypoint(); // Após a pausa, vai para o próximo ponto.

        isWaiting = false;
        agent.isStopped = false;
    }

    private void GoToNextWaypoint()
    {
        if (patrolPoints.Length == 0) return;

        // Avança para o próximo índice na lista, voltando ao início se chegar no final.
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // ... (ChasePlayer, AttackPlayer e o resto continuam iguais)

    private void ChasePlayer()
    {
        if (isWaiting)
        {
            isWaiting = false;
            StopAllCoroutines();
        }
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        agent.isStopped = true;

        Vector3 positionToLookAt = player.position;
        positionToLookAt.y = transform.position.y;
        transform.LookAt(positionToLookAt);

        if (!alreadyAttacked)
        {
            if (attackDamage <= 0)
            {
                Debug.LogWarning("AVISO: Dano de ataque do inimigo " + name + " é 0 ou menor.", this);
                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
                return;
            }

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
            else
            {
                Debug.LogError("ATAQUE FALHOU: Inimigo " + name + " não encontrou o script 'PlayerHealth' no objeto " + player.name, this);
            }

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}