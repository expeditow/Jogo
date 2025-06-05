// Importa os namespaces necess�rios do Unity.
// UnityEngine cont�m funcionalidades b�sicas do Unity.
// UnityEngine.AI � essencial para usar o NavMeshAgent para navega��o.
using UnityEngine;
using UnityEngine.AI;

// Declara��o da classe EnemyAI, que herda de MonoBehaviour.
// MonoBehaviour � a classe base da qual todos os scripts do Unity devem herdar para serem anexados a GameObjects.
public class EnemyAI : MonoBehaviour
{
    // O atributo [Header("Texto")] cria um t�tulo no Inspector do Unity para organizar as vari�veis.
    [Header("Refer�ncias Principais")]
    // Vari�vel p�blica para armazenar o componente NavMeshAgent do inimigo.
    // O NavMeshAgent � o que permite ao inimigo navegar pela NavMesh (malha de navega��o).
    public NavMeshAgent agent;
    // Vari�vel p�blica para armazenar o Transform do jogador.
    // O Transform cont�m informa��es sobre posi��o, rota��o e escala do objeto do jogador.
    public Transform player;

    [Header("Layers")]
    // LayerMasks s�o usadas para filtrar colis�es ou raycasts para layers espec�ficas.
    // whatIsGround pode ser usada para verificar se um ponto de patrulha est� em um ch�o v�lido.
    public LayerMask whatIsGround;
    // whatIsPlayer � usada para que o Physics.CheckSphere detecte apenas objetos na layer do jogador.
    public LayerMask whatIsPlayer;

    [Header("Configura��es de Patrulha por Waypoints")]
    // Array (lista) de Transforms. Cada Transform ser� a posi��o de um waypoint de patrulha.
    public Transform[] patrolPoints;
    // Vari�vel privada para guardar o �ndice do waypoint atual na lista patrolPoints.
    // Come�a em 0, que � o primeiro waypoint da lista.
    private int currentPatrolIndex = 0;

    // Vari�veis da patrulha aleat�ria original (comentadas pois n�o s�o usadas na patrulha por waypoints)
    // public Vector3 walkPoint;
    // bool walkPointSet;
    // public float walkPointRange;

    [Header("Configura��es de Persegui��o e Ataque")]
    // Raio de vis�o do inimigo. Se o jogador entrar neste raio, o inimigo o persegue.
    public float sightRange;
    // Raio de ataque do inimigo. Se o jogador estiver no raio de vis�o E neste raio, o inimigo ataca.
    public float attackRange;
    // Tempo (em segundos) entre cada ataque do inimigo. Define o cooldown do ataque.
    public float timeBetweenAttacks = 2f;
    // Flag booleana privada para controlar se o inimigo j� atacou e est� esperando o cooldown.
    private bool alreadyAttacked;

    [Header("Estados (Apenas para Debug Visual)")]
    // Vari�veis booleanas p�blicas para mostrar no Inspector se o jogador est� no raio de vis�o ou ataque.
    // S�o atualizadas no m�todo Update().
    public bool playerInSightRange;
    public bool playerInAttackRange;

    // Awake() � chamado uma vez quando o script � carregado (antes do Start()).
    // Ideal para inicializar vari�veis e refer�ncias.
    private void Awake()
    {
        // Tenta encontrar o GameObject do jogador na cena usando a Tag "Player".
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        // Verifica se o jogador foi encontrado.
        if (playerObject != null)
        {
            // Se encontrado, armazena o Transform do jogador na vari�vel 'player'.
            player = playerObject.transform;
        }
        else
        {
            // Se n�o encontrado, registra um erro no Console do Unity.
            Debug.LogError("INIMIGO: Jogador n�o encontrado! Certifique-se que ele tem a tag 'Player'.");
        }

        // Obt�m o componente NavMeshAgent anexado a este mesmo GameObject (o inimigo).
        agent = GetComponent<NavMeshAgent>();
        // Verifica se o componente NavMeshAgent foi encontrado.
        if (agent == null)
        {
            // Se n�o encontrado, registra um erro no Console.
            Debug.LogError("INIMIGO: Componente NavMeshAgent n�o encontrado neste GameObject: " + gameObject.name);
        }

        // Verifica se foram definidos waypoints de patrulha no Inspector.
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // Se n�o houver waypoints, avisa no Console que o inimigo pode ficar parado.
            Debug.LogWarning("INIMIGO: Nenhum ponto de patrulha (patrolPoints) atribu�do a: " + gameObject.name + ". Ele ficar� parado se n�o estiver perseguindo e n�o houver l�gica de ataque.");
        }
    }

    // Update() � chamado a cada frame do jogo.
    // Cont�m a l�gica principal que precisa ser verificada continuamente.
    private void Update()
    {
        // Verifica��o de seguran�a: se n�o houver refer�ncia ao jogador ou ao NavMeshAgent, interrompe a execu��o do Update.
        if (player == null || agent == null)
        {
            // Se o agente existir, mas o jogador n�o, manda o agente parar.
            if (agent != null) agent.isStopped = true;
            return; // Sai do m�todo Update.
        }

        // Usa Physics.CheckSphere para criar uma esfera invis�vel e verificar se o jogador (que deve estar na layer whatIsPlayer) est� dentro dela.
        // Isso determina se o jogador est� no raio de vis�o.
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        // Mesmo princ�pio, mas para o raio de ataque.
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // M�quina de estados simples baseada em 'if/else if' para decidir o comportamento do inimigo.
        if (!playerInSightRange && !playerInAttackRange) // Se o jogador N�O est� no raio de vis�o E N�O est� no raio de ataque.
        {
            // Chama o m�todo de Patrulha.
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange) // Se o jogador EST� no raio de vis�o MAS N�O no raio de ataque.
        {
            // Chama o m�todo de Persegui��o.
            ChasePlayer();
        }
        else if (playerInAttackRange && playerInSightRange) // Se o jogador EST� no raio de ataque E no raio de vis�o.
        {
            // Chama o m�todo de Ataque.
            AttackPlayer();
        }
    }

    // M�todo para controlar o comportamento de patrulha.
    private void Patroling()
    {
        // Verifica se existem waypoints definidos.
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            // Se n�o houver waypoints, o inimigo para.
            if (agent != null) agent.isStopped = true;
            return; // Sai do m�todo.
        }

        // Se existem waypoints, garante que o agente pode se mover (caso estivesse parado por algum motivo).
        if (agent != null) agent.isStopped = false;

        // Pega o Transform do waypoint atual.
        Transform targetWaypoint = patrolPoints[currentPatrolIndex];
        // Verifica��o de seguran�a: se o waypoint no �ndice atual for nulo (ex: foi destru�do ou n�o atribu�do corretamente).
        if (targetWaypoint == null)
        {
            Debug.LogError("INIMIGO: Ponto de patrulha (�ndice " + currentPatrolIndex + ") � nulo para " + gameObject.name + ". Indo para o pr�ximo.");
            GoToNextWaypoint(); // Tenta avan�ar para o pr�ximo waypoint para evitar erros cont�nuos.
            return; // Sai do m�todo.
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
            Debug.Log(gameObject.name + " est� atacando " + player.name); 
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