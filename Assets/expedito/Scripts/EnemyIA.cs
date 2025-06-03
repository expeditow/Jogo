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

        // Define o destino do NavMeshAgent para a posi��o do waypoint atual.
        // A verifica��o 'agent.destination != targetWaypoint.position' evita chamar SetDestination desnecessariamente a cada frame,
        // o que pode ser um pouco custoso. Chamamos apenas se o destino mudou ou n�o foi definido ainda.
        if (agent.destination != targetWaypoint.position)
        {
            agent.SetDestination(targetWaypoint.position);
        }

        // Verifica se o agente chegou perto do destino atual.
        // '!agent.pathPending' garante que o agente j� calculou um caminho (n�o est� "pensando" no caminho).
        // 'agent.remainingDistance < 0.5f' verifica se a dist�ncia restante para o destino � menor que 0.5 unidades.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Se chegou, chama o m�todo para ir ao pr�ximo waypoint.
            GoToNextWaypoint();
        }
    }

    // M�todo para atualizar o �ndice e ir para o pr�ximo waypoint.
    private void GoToNextWaypoint()
    {
        // Verifica��o de seguran�a para garantir que h� waypoints.
        if (patrolPoints.Length == 0)
            return;

        // Incrementa o �ndice do waypoint atual.
        // O operador '%' (m�dulo) � usado para criar um loop:
        // Se currentPatrolIndex for 0 e patrolPoints.Length for 3, (0+1)%3 = 1.
        // Se currentPatrolIndex for 1 e patrolPoints.Length for 3, (1+1)%3 = 2.
        // Se currentPatrolIndex for 2 e patrolPoints.Length for 3, (2+1)%3 = 0. (Volta para o in�cio).
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // O novo destino ser� definido na pr�xima chamada de Patroling().
    }

    // M�todo SearchWalkPoint da patrulha aleat�ria original (comentado, pois n�o � mais usado)
    /*
    private void SearchWalkPoint()
    {
        // Calcula um ponto aleat�rio no range de patrulha
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Verifica se o ponto � v�lido no ch�o (NavMesh)
        // Este Raycast � uma simplifica��o; para NavMesh, idealmente usar�amos NavMesh.SamplePosition
        if (Physics.Raycast(walkPoint + Vector3.up * 2f, -transform.up, 4f, whatIsGround))
            walkPointSet = true;
    }
    */

    // M�todo para controlar o comportamento de persegui��o.
    private void ChasePlayer()
    {
        // Se o NavMeshAgent existir.
        if (agent != null)
        {
            // Garante que o agente pode se mover (caso estivesse parado).
            agent.isStopped = false;
            // Define o destino do agente como a posi��o atual do jogador.
            agent.SetDestination(player.position);
        }
    }

    // M�todo para controlar o comportamento de ataque.
    private void AttackPlayer()
    {
        // Se o NavMeshAgent existir.
        if (agent != null)
        {
            // Para o movimento do agente. Ele chegou perto o suficiente para atacar.
            // agent.SetDestination(transform.position) tamb�m pararia o movimento em rela��o a um novo destino,
            // mas agent.isStopped = true; � mais direto para simplesmente parar.
            agent.isStopped = true;
        }

        // Faz o inimigo olhar na dire��o do jogador.
        transform.LookAt(player);

        // Verifica se o inimigo j� atacou (para respeitar o cooldown 'timeBetweenAttacks').
        if (!alreadyAttacked)
        {
            // --- SE��O PARA SUA L�GICA DE ATAQUE ---
            // Aqui voc� colocaria o que acontece quando o inimigo ataca.
            // Exemplos: causar dano, instanciar um proj�til, tocar uma anima��o de ataque, etc.
            Debug.Log(gameObject.name + " est� atacando " + player.name); // Exemplo de mensagem no console.

            // Exemplo comentado de como instanciar um proj�til (voc� precisaria de uma vari�vel 'projectile'):
            // Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            // rb.AddForce(transform.forward * 32f, ForceMode.Impulse); // For�a para frente
            // rb.AddForce(transform.up * 4f, ForceMode.Impulse);       // Uma pequena for�a para cima, para um arco

            // -----------------------------------------

            // Marca que o inimigo acabou de atacar.
            alreadyAttacked = true;
            // Chama o m�todo ResetAttack depois do tempo definido em 'timeBetweenAttacks'.
            // nameof(ResetAttack) � uma forma segura de passar o nome do m�todo como string, evitando erros de digita��o.
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    // M�todo para resetar o estado de ataque, permitindo que o inimigo ataque novamente.
    private void ResetAttack()
    {
        // Marca que o inimigo n�o est� mais no estado "j� atacou".
        alreadyAttacked = false;

        // Se o agente existir e o jogador ainda estiver vis�vel mas fora do alcance de ataque,
        // permite que o agente se mova novamente para continuar a persegui��o.
        if (agent != null && (playerInSightRange && !playerInAttackRange))
        {
            agent.isStopped = false;
        }
        // Se o jogador saiu completamente do campo de vis�o, o m�todo Update() cuidar� de voltar ao estado Patroling().
        // Se o jogador ainda estiver no alcance de ataque, o Update() chamar� AttackPlayer() novamente,
        // e como alreadyAttacked � false, um novo ataque ocorrer�.
    }

    // OnDrawGizmosSelected() � um m�todo especial do Unity chamado quando o objeto est� selecionado no editor.
    // � usado para desenhar Gizmos (ajudas visuais) na Scene View, que n�o aparecem no jogo final.
    // Muito �til para depurar raios de vis�o, �reas, etc.
    private void OnDrawGizmosSelected()
    {
        // Define a cor do Gizmo para amarelo.
        Gizmos.color = Color.yellow;
        // Desenha uma esfera de arame na posi��o do inimigo com o raio igual a 'sightRange'.
        Gizmos.DrawWireSphere(transform.position, sightRange);

        // Define a cor do Gizmo para vermelho.
        Gizmos.color = Color.red;
        // Desenha uma esfera de arame para o 'attackRange'.
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Desenha Gizmos para os waypoints de patrulha, se existirem.
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            // Define a cor para os waypoints e linhas entre eles.
            Gizmos.color = Color.blue;
            // Loop por todos os waypoints definidos.
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                // Verifica se o waypoint atual no array n�o � nulo (n�o foi destru�do ou est� vazio).
                if (patrolPoints[i] != null)
                {
                    // Desenha uma pequena esfera de arame na posi��o de cada waypoint.
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    // Se n�o for o primeiro waypoint e o waypoint anterior tamb�m existir,
                    // desenha uma linha entre o waypoint anterior e o atual.
                    if (i > 0 && patrolPoints[i - 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
                    }
                    // Se for o �ltimo waypoint e o primeiro waypoint existir,
                    // desenha uma linha do �ltimo waypoint de volta para o primeiro, para visualizar o loop da patrulha.
                    if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
            // Desenha uma linha do inimigo para o seu waypoint de destino atual.
            // Verifica se o �ndice atual � v�lido e se o waypoint nesse �ndice existe.
            if (currentPatrolIndex < patrolPoints.Length && patrolPoints[currentPatrolIndex] != null)
            {
                // Define a cor para a linha de destino.
                Gizmos.color = Color.cyan;
                // Desenha a linha da posi��o atual do inimigo at� a posi��o do waypoint alvo.
                Gizmos.DrawLine(transform.position, patrolPoints[currentPatrolIndex].position);
            }
        }
    }
}