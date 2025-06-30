// Importa os namespaces necessários do Unity.
// UnityEngine contém funcionalidades básicas do Unity.
// UnityEngine.AI é essencial para usar o NavMeshAgent para navegação.
using UnityEngine;
using UnityEngine.AI;

// Declaração da classe EnemyAI, que herda de MonoBehaviour.
// MonoBehaviour é a classe base da qual todos os scripts do Unity devem herdar para serem anexados a GameObjects.
public class EnemyAI : MonoBehaviour
{
    // O atributo [Header("Texto")] cria um título no Inspector do Unity para organizar as variáveis.
    [Header("Referências Principais")]
    // Variável pública para armazenar o componente NavMeshAgent do inimigo.
    // O NavMeshAgent é o que permite ao inimigo navegar pela NavMesh (malha de navegação).
    public NavMeshAgent agent;
    // Variável pública para armazenar o Transform do jogador.
    // O Transform contém informações sobre posição, rotação e escala do objeto do jogador.
    public Transform player;

    [Header("Layers")]
    // LayerMasks são usadas para filtrar colisões ou raycasts para layers específicas.
    // whatIsGround pode ser usada para verificar se um ponto de patrulha está em um chão válido.
    public LayerMask whatIsGround;
    // whatIsPlayer é usada para que o Physics.CheckSphere detecte apenas objetos na layer do jogador.
    public LayerMask whatIsPlayer;

    [Header("Configurações de Patrulha por Waypoints")]
    // Array (lista) de Transforms. Cada Transform será a posição de um waypoint de patrulha.
    public Transform[] patrolPoints;
    // Variável privada para guardar o índice do waypoint atual na lista patrolPoints.
    // Começa em 0, que é o primeiro waypoint da lista.
    private int currentPatrolIndex = 0;

    // Variáveis da patrulha aleatória original (comentadas pois não são usadas na patrulha por waypoints)
    // public Vector3 walkPoint;
    // bool walkPointSet;
    // public float walkPointRange;

    [Header("Configurações de Perseguição e Ataque")]
    // Raio de visão do inimigo. Se o jogador entrar neste raio, o inimigo o persegue.
    public float sightRange;
    // Raio de ataque do inimigo. Se o jogador estiver no raio de visão E neste raio, o inimigo ataca.
    public float attackRange;
    // Tempo (em segundos) entre cada ataque do inimigo. Define o cooldown do ataque.
    public float timeBetweenAttacks = 2f;
    // Flag booleana privada para controlar se o inimigo já atacou e está esperando o cooldown.
    private bool alreadyAttacked;

    [Header("Estados (Apenas para Debug Visual)")]
    // Variáveis booleanas públicas para mostrar no Inspector se o jogador está no raio de visão ou ataque.
    // São atualizadas no método Update().
    public bool playerInSightRange;
    public bool playerInAttackRange;

    // Awake() é chamado uma vez quando o script é carregado (antes do Start()).
    // Ideal para inicializar variáveis e referências.
    private void Awake()
    {
        // Tenta encontrar o GameObject do jogador na cena usando a Tag "Player".
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        // Verifica se o jogador foi encontrado.
        if (playerObject != null)
        {
            // Se encontrado, armazena o Transform do jogador na variável 'player'.
            player = playerObject.transform;
        }
        else
        {
            // Se não encontrado, registra um erro no Console do Unity.
            Debug.LogError("INIMIGO: Jogador não encontrado! Certifique-se que ele tem a tag 'Player'.");
        }

        // Obtém o componente NavMeshAgent anexado a este mesmo GameObject (o inimigo).
        agent = GetComponent<NavMeshAgent>();
        // Verifica se o componente NavMeshAgent foi encontrado.
        if (agent == null)
        {
            // Se não encontrado, registra um erro no Console.
            Debug.LogError("INIMIGO: Componente NavMeshAgent não encontrado neste GameObject: " + gameObject.name);
        }

        // Verifica se foram definidos waypoints de patrulha no Inspector.
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // Se não houver waypoints, avisa no Console que o inimigo pode ficar parado.
            Debug.LogWarning("INIMIGO: Nenhum ponto de patrulha (patrolPoints) atribuído a: " + gameObject.name + ". Ele ficará parado se não estiver perseguindo e não houver lógica de ataque.");
        }
    }

    // Update() é chamado a cada frame do jogo.
    // Contém a lógica principal que precisa ser verificada continuamente.
    private void Update()
    {
        // Verificação de segurança: se não houver referência ao jogador ou ao NavMeshAgent, interrompe a execução do Update.
        if (player == null || agent == null)
        {
            // Se o agente existir, mas o jogador não, manda o agente parar.
            if (agent != null) agent.isStopped = true;
            return; // Sai do método Update.
        }

        // Usa Physics.CheckSphere para criar uma esfera invisível e verificar se o jogador (que deve estar na layer whatIsPlayer) está dentro dela.
        // Isso determina se o jogador está no raio de visão.
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        // Mesmo princípio, mas para o raio de ataque.
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Máquina de estados simples baseada em 'if/else if' para decidir o comportamento do inimigo.
        if (!playerInSightRange && !playerInAttackRange) // Se o jogador NÃO está no raio de visão E NÃO está no raio de ataque.
        {
            // Chama o método de Patrulha.
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange) // Se o jogador ESTÁ no raio de visão MAS NÃO no raio de ataque.
        {
            // Chama o método de Perseguição.
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange) // Se o jogador ESTÁ no raio de ataque E no raio de visão.
        {
            // Chama o método de Ataque.
            AttackPlayer();
        }
    }

    // Método para controlar o comportamento de patrulha.
    private void Patroling()
    {
        // Verifica se existem waypoints definidos.
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // Se não houver waypoints, o inimigo para.
            if (agent != null) agent.isStopped = true;
            return; // Sai do método.
        }

        // Se existem waypoints, garante que o agente pode se mover (caso estivesse parado por algum motivo).
        if (agent != null) agent.isStopped = false;

        // Pega o Transform do waypoint atual.
        Transform targetWaypoint = patrolPoints[currentPatrolIndex];
        // Verificação de segurança: se o waypoint no índice atual for nulo (ex: foi destruído ou não atribuído corretamente).
        if (targetWaypoint == null)
        {
            Debug.LogError("INIMIGO: Ponto de patrulha (índice " + currentPatrolIndex + ") é nulo para " + gameObject.name + ". Indo para o próximo.");
            GoToNextWaypoint(); // Tenta avançar para o próximo waypoint para evitar erros contínuos.
            return; // Sai do método.
        }
        if (agent.destination != targetWaypoint.position)
        {
            agent.SetDestination(targetWaypoint.position);
        }

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    private void GoToNextWaypoint()
    {

        if (patrolPoints.Length == 0)
            return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

    }

    private void ChasePlayer()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    private void AttackPlayer()
    {
        if (agent != null)
        {
            agent.isStopped = true;
        }
        transform.LookAt(player);
        if (!alreadyAttacked)
        {
            Debug.Log(gameObject.name + " está atacando " + player.name); 
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
        if (agent != null && (playerInSightRange && !playerInAttackRange))
        {
            agent.isStopped = false;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    if (i > 0 && patrolPoints[i - 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
                    }
                    if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
            if (currentPatrolIndex < patrolPoints.Length && patrolPoints[currentPatrolIndex] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, patrolPoints[currentPatrolIndex].position);
            }
        }
    }
}