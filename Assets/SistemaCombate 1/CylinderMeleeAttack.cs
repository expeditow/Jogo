// CylinderMeleeAttack.cs
using System.Collections;
using System.Collections.Generic;
// using UnityEditor; // Comente ou envolva com #if UNITY_EDITOR se não estiver no Editor folder.
using UnityEngine;

public class CylinderMeleeAttack : MonoBehaviour
{
    [Header("Equipamento")]
    [Tooltip("Arraste o Asset de WeaponStats da arma atualmente equipada aqui. Define as estatísticas e a lógica de ataque.")]
    public WeaponStats currentEquippedWeapon; // Referência ao ScriptableObject WeaponStats

    [Header("Pontos de Referência")]
    [Tooltip("Objeto vazio que define a origem e direção do seu ataque (ex: na frente da mira do cilindro).")]
    public Transform attackOriginPoint;

    [Header("Referências Visuais/Sonoras Comuns do Jogador")]
    [Tooltip("Prefab de efeito de partículas ou visual genérico para o impacto do ataque. Usado pela lógica da arma.")]
    public GameObject hitEffectPrefab;
    [Tooltip("Som genérico reproduzido no momento que o ataque é ativado. Usado pela lógica da arma.")]
    public AudioClip defaultAttackSound; // Som padrão se a arma não tiver um

    private float nextAttackTime = 0f;
    private bool isAttacking = false; // Flag para evitar múltiplos ataques durante uma mesma animação/rotina

    private AudioSource audioSource;
    private WeaponAttackLogic activeWeaponLogic; // Referência à instância da lógica de ataque da arma atualmente ativa.

    [Header("Gerenciamento de Estamina")]
    public PlayerMovement playerMovement; // Assumindo que PlayerMovement existe

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PLAYER: Componente PlayerMovement não encontrado no mesmo GameObject que CylinderMeleeAttack! A estamina não será gerenciada.", this);
        }

        if (attackOriginPoint == null)
        {
            Debug.LogError("PLAYER: Attack Origin Point não configurado no Cilindro do Jogador! Ataque pode não funcionar.", this);
            attackOriginPoint = this.transform; // Fallback: usa o próprio transform do jogador
        }

        // Equipa a arma padrão ao iniciar o jogo.
        // Ou apenas garante que activeWeaponLogic seja inicializado.
        if (currentEquippedWeapon != null)
        {
            SetCurrentWeaponStats(currentEquippedWeapon); // Chama o novo método para configurar a arma padrão
        }
        else
        {
            Debug.LogWarning("PLAYER: Nenhuma arma padrão definida no Inspector. O ataque pode não funcionar corretamente no início. Usando valores padrão.", this);
            SetCurrentWeaponStats(null); // Garante que a lógica de ataque seja limpa e use defaults.
        }
    }

    // OnValidate para preview de Gizmos no editor
    void OnValidate()
    {
        // Use #if UNITY_EDITOR para garantir que este código só rode no editor
#if UNITY_EDITOR
        if (Application.isPlaying || this == null) return;

        // Usar EditorApplication.delayCall para manipular GameObjects no OnValidate é mais seguro
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Objeto pode ter sido destruído enquanto esperava

            // Destrói qualquer lógica de preview antiga
            WeaponAttackLogic oldLogic = GetComponentInChildren<WeaponAttackLogic>();
            if (oldLogic != null && oldLogic.gameObject.name == "ActiveWeaponLogic_EditorPreview")
            {
                DestroyImmediate(oldLogic.gameObject);
            }
            activeWeaponLogic = null; // Limpa a referência local

            // Cria o novo objeto de preview se uma arma válida estiver selecionada
            if (currentEquippedWeapon != null && currentEquippedWeapon.attackLogicPrefab != null)
            {
                GameObject logicGO = Instantiate(currentEquippedWeapon.attackLogicPrefab, transform);
                logicGO.name = "ActiveWeaponLogic_EditorPreview";
                // HideFlags garantem que este objeto não polua a hierarquia e não seja salvo na cena.
                logicGO.hideFlags = HideFlags.HideAndDontSave;
                activeWeaponLogic = logicGO.GetComponent<WeaponAttackLogic>();

                if (activeWeaponLogic != null && attackOriginPoint != null)
                {
                    // Garante que o AudioSource exista para a inicialização no editor
                    AudioSource source = GetComponent<AudioSource>();
                    if (source == null) source = gameObject.AddComponent<AudioSource>();

                    // Inicializa a lógica para o preview do Gizmo com valores da arma
                    activeWeaponLogic.Initialize(attackOriginPoint, hitEffectPrefab, defaultAttackSound, source, currentEquippedWeapon.baseDamage, currentEquippedWeapon.baseAttackRange);
                }
            }
        };
#endif
    }

    // --- NOVO MÉTODO: Define as estatísticas da arma a ser usada (Chamado pelo InventorySystem) ---
    // Dentro da classe CylinderMeleeAttack.cs
    public void SetCurrentWeaponStats(WeaponStats stats)
    {
        Debug.Log($"[CMA_SET_WEAPON] --- INÍCIO: SetCurrentWeaponStats chamado com stats: {(stats != null ? stats.name : "NULL")} ---");
        currentEquippedWeapon = stats; // Atribui o Asset WeaponStats equipado

        // Limpa a lógica ativa anterior se houver
        if (activeWeaponLogic != null)
        {
            Debug.Log("[CMA_SET_WEAPON] Destruindo lógica de ataque anterior.");
            Destroy(activeWeaponLogic.gameObject);
            activeWeaponLogic = null;
        }

        // Instancia a nova lógica de ataque se uma arma válida for fornecida
        if (currentEquippedWeapon != null && currentEquippedWeapon.attackLogicPrefab != null)
        {
            Debug.Log("[CMA_SET_WEAPON] currentEquippedWeapon e attackLogicPrefab não são nulos. Tentando instanciar nova lógica.");
            GameObject logicGO = Instantiate(currentEquippedWeapon.attackLogicPrefab, transform);
            logicGO.name = "ActiveWeaponLogic_" + currentEquippedWeapon.name; // Nome para depuração
            activeWeaponLogic = logicGO.GetComponent<WeaponAttackLogic>(); // Tenta pegar o componente

            if (activeWeaponLogic != null)
            {
                Debug.Log($"[CMA_SET_WEAPON] Componente WeaponAttackLogic encontrado na nova lógica. Inicializando.");
                activeWeaponLogic.Initialize(
                    attackOriginPoint,
                    hitEffectPrefab,
                    (currentEquippedWeapon.attackSound != null) ? currentEquippedWeapon.attackSound : defaultAttackSound,
                    audioSource,
                    currentEquippedWeapon.baseDamage,
                    currentEquippedWeapon.baseAttackRange
                );
                Debug.Log($"[CMA_SET_WEAPON] Lógica de ataque '{activeWeaponLogic.name}' instanciada e inicializada com SUCESSO!");
            }
            else
            {
                Debug.LogError($"[CMA_SET_WEAPON] ERRO FATAL: O Prefab de lógica de ataque '{currentEquippedWeapon.attackLogicPrefab.name}' NÃO contém um componente WeaponAttackLogic! Verifique o prefab. (6)");
            }
        }
        else // Se currentEquippedWeapon ou attackLogicPrefab são nulos
        {
            Debug.LogWarning("[CMA_SET_WEAPON] AVISO: currentEquippedWeapon ou attackLogicPrefab é nulo. Nenhuma lógica de ataque ativa será definida.");
            if (currentEquippedWeapon != null)
            {
                Debug.LogWarning($"[CMA_SET_WEAPON] Detalhes: currentEquippedWeapon: {currentEquippedWeapon.name}, attackLogicPrefab: {(currentEquippedWeapon.attackLogicPrefab != null ? currentEquippedWeapon.attackLogicPrefab.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning("[CMA_SET_WEAPON] Detalhes: currentEquippedWeapon é NULL.");
            }
        }
        Debug.Log($"[CMA_SET_WEAPON] --- FIM: SetCurrentWeaponStats ---");
    }
    // --- FIM DO NOVO MÉTODO ---


    void Update()
    {
        // Pega as estatísticas da arma equipada, ou usa valores padrão
        float actualCooldown = (currentEquippedWeapon != null) ? currentEquippedWeapon.baseAttackCooldown : 0.5f;
        float actualActivationDelay = (currentEquippedWeapon != null) ? currentEquippedWeapon.baseAttackActivationDelay : 0.25f;
        float actualStaminaCost = (currentEquippedWeapon != null) ? currentEquippedWeapon.attackStaminaCost : 0f;

        // Lógica de ataque primário (ex: botão esquerdo do mouse)
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log($"PLAYER: Botão de ataque detectado! (Time: {Time.time}, NextAttackTime: {nextAttackTime}, IsAttacking: {isAttacking})");

            if (Time.time < nextAttackTime)
            {
                Debug.Log("PLAYER: Ataque em cooldown. Não executando.");
                return;
            }
            if (isAttacking)
            {
                Debug.Log("PLAYER: Já atacando. Não executando novamente.");
                return;
            }

            // Verifica se há uma lógica de ataque equipada e se o jogador tem estamina
            if (activeWeaponLogic == null)
            {
                Debug.LogWarning("PLAYER: Nenhuma lógica de ataque ativa. Não é possível atacar. Certifique-se de que a arma tem um prefab de lógica de ataque.", this);
                return;
            }

            if (playerMovement != null && playerMovement.GetCurrentStamina() < actualStaminaCost)
            {
                Debug.Log("PLAYER: Estamina insuficiente para atacar. Estamina atual: " + playerMovement.GetCurrentStamina() + ", Custo: " + actualStaminaCost);
                return;
            }

            Debug.Log("PLAYER: Iniciando coroutine PerformMeleeAttackWithDelay().");
            StartCoroutine(PerformMeleeAttackWithDelay(actualActivationDelay, actualStaminaCost)); // Passa delay e custo de estamina
            nextAttackTime = Time.time + actualCooldown; // Define o cooldown baseado no cooldown real da arma
        }

        // Lógica de uso de item equipado (botão direito do mouse)
        if (Input.GetButtonDown("Fire2"))
        {
            Debug.Log("PLAYER: Botão direito do mouse (Fire2) clicado. Tentando usar item equipado.");
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.UseEquippedItem();
            }
            else
            {
                Debug.LogWarning("PLAYER: InventorySystem.Instance não encontrado. Não é possível usar item.");
            }
        }
    }

    IEnumerator PerformMeleeAttackWithDelay(float delay, float staminaCost) // Recebe delay e staminaCost
    {
        isAttacking = true;
        Debug.Log("PLAYER: Coroutine PerformMeleeAttackWithDelay() iniciada. Aguardando delay...");

        if (playerMovement != null)
        {
            playerMovement.ConsumeStamina(staminaCost); // Usa o staminaCost passado
            Debug.Log($"PLAYER: Estamina consumida: {staminaCost}.");
        }

        yield return new WaitForSeconds(delay); // Usa o delay passado

        Debug.Log("PLAYER: Delay concluído. Tentando chamar PerformAttack na lógica da arma.");

        if (activeWeaponLogic != null)
        {
            activeWeaponLogic.PerformAttack(); // Chama PerformAttack sem parâmetros
            Debug.Log("PLAYER: PerformAttack na lógica da arma chamado com sucesso.");
        }
        else
        {
            Debug.LogError("PLAYER: ERRO CRÍTICO: activeWeaponLogic é NULO após o delay. A lógica da arma foi destruída ou nunca foi atribuída corretamente. O ataque não será executado.", this);
        }

        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        // A lógica de preview de Gizmo é tratada pelo OnValidate.
        // Aqui, apenas desenha se as referências estiverem válidas.
        if (activeWeaponLogic == null || attackOriginPoint == null)
        {
            Gizmos.color = Color.grey;
            // Desenha uma esfera simples se não houver lógica de arma ativa para indicar o player
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            return;
        }

        // Usa as estatísticas da arma equipada para desenhar o Gizmo
        float gizmoRange = (currentEquippedWeapon != null) ? currentEquippedWeapon.baseAttackRange : 1.0f; // Fallback para 1.0f

        activeWeaponLogic.DrawAttackGizmos(attackOriginPoint, gizmoRange); // Passa o alcance atual
    }
}