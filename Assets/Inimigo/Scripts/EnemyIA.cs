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

        // Define o destino do NavMeshAgent para a posição do waypoint atual.
        // A verificação 'agent.destination != targetWaypoint.position' evita chamar SetDestination desnecessariamente a cada frame,
        // o que pode ser um pouco custoso. Chamamos apenas se o destino mudou ou não foi definido ainda.
        if (agent.destination != targetWaypoint.position)
        {
            agent.SetDestination(targetWaypoint.position);
        }

        // Verifica se o agente chegou perto do destino atual.
        // '!agent.pathPending' garante que o agente já calculou um caminho (não está "pensando" no caminho).
        // 'agent.remainingDistance < 0.5f' verifica se a distância restante para o destino é menor que 0.5 unidades.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Se chegou, chama o método para ir ao próximo waypoint.
            GoToNextWaypoint();
        }
    }

    // Método para atualizar o índice e ir para o próximo waypoint.
    private void GoToNextWaypoint()
    {
        // Verificação de segurança para garantir que há waypoints.
        if (patrolPoints.Length == 0)
            return;

        // Incrementa o índice do waypoint atual.
        // O operador '%' (módulo) é usado para criar um loop:
        // Se currentPatrolIndex for 0 e patrolPoints.Length for 3, (0+1)%3 = 1.
        // Se currentPatrolIndex for 1 e patrolPoints.Length for 3, (1+1)%3 = 2.
        // Se currentPatrolIndex for 2 e patrolPoints.Length for 3, (2+1)%3 = 0. (Volta para o início).
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;

        // O novo destino será definido na próxima chamada de Patroling().
    }

    // Método SearchWalkPoint da patrulha aleatória original (comentado, pois não é mais usado)
    /*
    private void SearchWalkPoint()
    {
        // Calcula um ponto aleatório no range de patrulha
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Verifica se o ponto é válido no chão (NavMesh)
        // Este Raycast é uma simplificação; para NavMesh, idealmente usaríamos NavMesh.SamplePosition
        if (Physics.Raycast(walkPoint + Vector3.up * 2f, -transform.up, 4f, whatIsGround))
            walkPointSet = true;
    }
    */

    // Método para controlar o comportamento de perseguição.
    private void ChasePlayer()
    {
        // Se o NavMeshAgent existir.
        if (agent != null)
        {
            // Garante que o agente pode se mover (caso estivesse parado).
            agent.isStopped = false;
            // Define o destino do agente como a posição atual do jogador.
            agent.SetDestination(player.position);
        }
    }

    // Método para controlar o comportamento de ataque.
    private void AttackPlayer()
    {
        // Se o NavMeshAgent existir.
        if (agent != null)
        {
            // Para o movimento do agente. Ele chegou perto o suficiente para atacar.
            // agent.SetDestination(transform.position) também pararia o movimento em relação a um novo destino,
            // mas agent.isStopped = true; é mais direto para simplesmente parar.
            agent.isStopped = true;
        }

        // Faz o inimigo olhar na direção do jogador.
        transform.LookAt(player);

        // Verifica se o inimigo já atacou (para respeitar o cooldown 'timeBetweenAttacks').
        if (!alreadyAttacked)
        {
            // --- SEÇÃO PARA SUA LÓGICA DE ATAQUE ---
            // Aqui você colocaria o que acontece quando o inimigo ataca.
            // Exemplos: causar dano, instanciar um projétil, tocar uma animação de ataque, etc.
            Debug.Log(gameObject.name + " está atacando " + player.name); // Exemplo de mensagem no console.

            // Exemplo comentado de como instanciar um projétil (você precisaria de uma variável 'projectile'):
            // Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            // rb.AddForce(transform.forward * 32f, ForceMode.Impulse); // Força para frente
            // rb.AddForce(transform.up * 4f, ForceMode.Impulse);       // Uma pequena força para cima, para um arco

            // -----------------------------------------

            // Marca que o inimigo acabou de atacar.
            alreadyAttacked = true;
            // Chama o método ResetAttack depois do tempo definido em 'timeBetweenAttacks'.
            // nameof(ResetAttack) é uma forma segura de passar o nome do método como string, evitando erros de digitação.
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    // Método para resetar o estado de ataque, permitindo que o inimigo ataque novamente.
    private void ResetAttack()
    {
        // Marca que o inimigo não está mais no estado "já atacou".
        alreadyAttacked = false;

        // Se o agente existir e o jogador ainda estiver visível mas fora do alcance de ataque,
        // permite que o agente se mova novamente para continuar a perseguição.
        if (agent != null && (playerInSightRange && !playerInAttackRange))
        {
            agent.isStopped = false;
        }
        // Se o jogador saiu completamente do campo de visão, o método Update() cuidará de voltar ao estado Patroling().
        // Se o jogador ainda estiver no alcance de ataque, o Update() chamará AttackPlayer() novamente,
        // e como alreadyAttacked é false, um novo ataque ocorrerá.
    }

    // OnDrawGizmosSelected() é um método especial do Unity chamado quando o objeto está selecionado no editor.
    // É usado para desenhar Gizmos (ajudas visuais) na Scene View, que não aparecem no jogo final.
    // Muito útil para depurar raios de visão, áreas, etc.
    private void OnDrawGizmosSelected()
    {
        // Define a cor do Gizmo para amarelo.
        Gizmos.color = Color.yellow;
        // Desenha uma esfera de arame na posição do inimigo com o raio igual a 'sightRange'.
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
                // Verifica se o waypoint atual no array não é nulo (não foi destruído ou está vazio).
                if (patrolPoints[i] != null)
                {
                    // Desenha uma pequena esfera de arame na posição de cada waypoint.
                    Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);
                    // Se não for o primeiro waypoint e o waypoint anterior também existir,
                    // desenha uma linha entre o waypoint anterior e o atual.
                    if (i > 0 && patrolPoints[i - 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
                    }
                    // Se for o último waypoint e o primeiro waypoint existir,
                    // desenha uma linha do último waypoint de volta para o primeiro, para visualizar o loop da patrulha.
                    if (i == patrolPoints.Length - 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }
                }
            }
            // Desenha uma linha do inimigo para o seu waypoint de destino atual.
            // Verifica se o índice atual é válido e se o waypoint nesse índice existe.
            if (currentPatrolIndex < patrolPoints.Length && patrolPoints[currentPatrolIndex] != null)
            {
                // Define a cor para a linha de destino.
                Gizmos.color = Color.cyan;
                // Desenha a linha da posição atual do inimigo até a posição do waypoint alvo.
                Gizmos.DrawLine(transform.position, patrolPoints[currentPatrolIndex].position);
            }
        }
    }
}