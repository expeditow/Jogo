// CylinderMeleeAttack.cs
using System.Collections;
using System.Collections.Generic;
// using UnityEditor; // Comente ou envolva com #if UNITY_EDITOR se n�o estiver no Editor folder.
using UnityEngine;

public class CylinderMeleeAttack : MonoBehaviour
{
    [Header("Equipamento")]
    [Tooltip("Arraste o Asset de WeaponStats da arma atualmente equipada aqui. Define as estat�sticas e a l�gica de ataque.")]
    public WeaponStats currentEquippedWeapon; // Refer�ncia ao ScriptableObject WeaponStats

    [Header("Pontos de Refer�ncia")]
    [Tooltip("Objeto vazio que define a origem e dire��o do seu ataque (ex: na frente da mira do cilindro).")]
    public Transform attackOriginPoint;

    [Header("Refer�ncias Visuais/Sonoras Comuns do Jogador")]
    [Tooltip("Prefab de efeito de part�culas ou visual gen�rico para o impacto do ataque. Usado pela l�gica da arma.")]
    public GameObject hitEffectPrefab;
    [Tooltip("Som gen�rico reproduzido no momento que o ataque � ativado. Usado pela l�gica da arma.")]
    public AudioClip defaultAttackSound; // Som padr�o se a arma n�o tiver um

    private float nextAttackTime = 0f;
    private bool isAttacking = false; // Flag para evitar m�ltiplos ataques durante uma mesma anima��o/rotina

    private AudioSource audioSource;
    private WeaponAttackLogic activeWeaponLogic; // Refer�ncia � inst�ncia da l�gica de ataque da arma atualmente ativa.

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
            Debug.LogError("PLAYER: Componente PlayerMovement n�o encontrado no mesmo GameObject que CylinderMeleeAttack! A estamina n�o ser� gerenciada.", this);
        }

        if (attackOriginPoint == null)
        {
            Debug.LogError("PLAYER: Attack Origin Point n�o configurado no Cilindro do Jogador! Ataque pode n�o funcionar.", this);
            attackOriginPoint = this.transform; // Fallback: usa o pr�prio transform do jogador
        }

        // Equipa a arma padr�o ao iniciar o jogo.
        // Ou apenas garante que activeWeaponLogic seja inicializado.
        if (currentEquippedWeapon != null)
        {
            SetCurrentWeaponStats(currentEquippedWeapon); // Chama o novo m�todo para configurar a arma padr�o
        }
        else
        {
            Debug.LogWarning("PLAYER: Nenhuma arma padr�o definida no Inspector. O ataque pode n�o funcionar corretamente no in�cio. Usando valores padr�o.", this);
            SetCurrentWeaponStats(null); // Garante que a l�gica de ataque seja limpa e use defaults.
        }
    }

    // OnValidate para preview de Gizmos no editor
    void OnValidate()
    {
        // Use #if UNITY_EDITOR para garantir que este c�digo s� rode no editor
#if UNITY_EDITOR
        if (Application.isPlaying || this == null) return;

        // Usar EditorApplication.delayCall para manipular GameObjects no OnValidate � mais seguro
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return; // Objeto pode ter sido destru�do enquanto esperava

            // Destr�i qualquer l�gica de preview antiga
            WeaponAttackLogic oldLogic = GetComponentInChildren<WeaponAttackLogic>();
            if (oldLogic != null && oldLogic.gameObject.name == "ActiveWeaponLogic_EditorPreview")
            {
                DestroyImmediate(oldLogic.gameObject);
            }
            activeWeaponLogic = null; // Limpa a refer�ncia local

            // Cria o novo objeto de preview se uma arma v�lida estiver selecionada
            if (currentEquippedWeapon != null && currentEquippedWeapon.attackLogicPrefab != null)
            {
                GameObject logicGO = Instantiate(currentEquippedWeapon.attackLogicPrefab, transform);
                logicGO.name = "ActiveWeaponLogic_EditorPreview";
                // HideFlags garantem que este objeto n�o polua a hierarquia e n�o seja salvo na cena.
                logicGO.hideFlags = HideFlags.HideAndDontSave;
                activeWeaponLogic = logicGO.GetComponent<WeaponAttackLogic>();

                if (activeWeaponLogic != null && attackOriginPoint != null)
                {
                    // Garante que o AudioSource exista para a inicializa��o no editor
                    AudioSource source = GetComponent<AudioSource>();
                    if (source == null) source = gameObject.AddComponent<AudioSource>();

                    // Inicializa a l�gica para o preview do Gizmo com valores da arma
                    activeWeaponLogic.Initialize(attackOriginPoint, hitEffectPrefab, defaultAttackSound, source, currentEquippedWeapon.baseDamage, currentEquippedWeapon.baseAttackRange);
                }
            }
        };
#endif
    }

    // --- NOVO M�TODO: Define as estat�sticas da arma a ser usada (Chamado pelo InventorySystem) ---
    // Dentro da classe CylinderMeleeAttack.cs
    public void SetCurrentWeaponStats(WeaponStats stats)
    {
        Debug.Log($"[CMA_SET_WEAPON] --- IN�CIO: SetCurrentWeaponStats chamado com stats: {(stats != null ? stats.name : "NULL")} ---");
        currentEquippedWeapon = stats; // Atribui o Asset WeaponStats equipado

        // Limpa a l�gica ativa anterior se houver
        if (activeWeaponLogic != null)
        {
            Debug.Log("[CMA_SET_WEAPON] Destruindo l�gica de ataque anterior.");
            Destroy(activeWeaponLogic.gameObject);
            activeWeaponLogic = null;
        }

        // Instancia a nova l�gica de ataque se uma arma v�lida for fornecida
        if (currentEquippedWeapon != null && currentEquippedWeapon.attackLogicPrefab != null)
        {
            Debug.Log("[CMA_SET_WEAPON] currentEquippedWeapon e attackLogicPrefab n�o s�o nulos. Tentando instanciar nova l�gica.");
            GameObject logicGO = Instantiate(currentEquippedWeapon.attackLogicPrefab, transform);
            logicGO.name = "ActiveWeaponLogic_" + currentEquippedWeapon.name; // Nome para depura��o
            activeWeaponLogic = logicGO.GetComponent<WeaponAttackLogic>(); // Tenta pegar o componente

            if (activeWeaponLogic != null)
            {
                Debug.Log($"[CMA_SET_WEAPON] Componente WeaponAttackLogic encontrado na nova l�gica. Inicializando.");
                activeWeaponLogic.Initialize(
                    attackOriginPoint,
                    hitEffectPrefab,
                    (currentEquippedWeapon.attackSound != null) ? currentEquippedWeapon.attackSound : defaultAttackSound,
                    audioSource,
                    currentEquippedWeapon.baseDamage,
                    currentEquippedWeapon.baseAttackRange
                );
                Debug.Log($"[CMA_SET_WEAPON] L�gica de ataque '{activeWeaponLogic.name}' instanciada e inicializada com SUCESSO!");
            }
            else
            {
                Debug.LogError($"[CMA_SET_WEAPON] ERRO FATAL: O Prefab de l�gica de ataque '{currentEquippedWeapon.attackLogicPrefab.name}' N�O cont�m um componente WeaponAttackLogic! Verifique o prefab. (6)");
            }
        }
        else // Se currentEquippedWeapon ou attackLogicPrefab s�o nulos
        {
            Debug.LogWarning("[CMA_SET_WEAPON] AVISO: currentEquippedWeapon ou attackLogicPrefab � nulo. Nenhuma l�gica de ataque ativa ser� definida.");
            if (currentEquippedWeapon != null)
            {
                Debug.LogWarning($"[CMA_SET_WEAPON] Detalhes: currentEquippedWeapon: {currentEquippedWeapon.name}, attackLogicPrefab: {(currentEquippedWeapon.attackLogicPrefab != null ? currentEquippedWeapon.attackLogicPrefab.name : "NULL")}");
            }
            else
            {
                Debug.LogWarning("[CMA_SET_WEAPON] Detalhes: currentEquippedWeapon � NULL.");
            }
        }
        Debug.Log($"[CMA_SET_WEAPON] --- FIM: SetCurrentWeaponStats ---");
    }
    // --- FIM DO NOVO M�TODO ---


    void Update()
    {
        // Pega as estat�sticas da arma equipada, ou usa valores padr�o
        float actualCooldown = (currentEquippedWeapon != null) ? currentEquippedWeapon.baseAttackCooldown : 0.5f;
        float actualActivationDelay = (currentEquippedWeapon != null) ? currentEquippedWeapon.baseAttackActivationDelay : 0.25f;
        float actualStaminaCost = (currentEquippedWeapon != null) ? currentEquippedWeapon.attackStaminaCost : 0f;

        // L�gica de ataque prim�rio (ex: bot�o esquerdo do mouse)
        if (Input.GetButtonDown("Fire1"))
        {
            Debug.Log($"PLAYER: Bot�o de ataque detectado! (Time: {Time.time}, NextAttackTime: {nextAttackTime}, IsAttacking: {isAttacking})");

            if (Time.time < nextAttackTime)
            {
                Debug.Log("PLAYER: Ataque em cooldown. N�o executando.");
                return;
            }
            if (isAttacking)
            {
                Debug.Log("PLAYER: J� atacando. N�o executando novamente.");
                return;
            }

            // Verifica se h� uma l�gica de ataque equipada e se o jogador tem estamina
            if (activeWeaponLogic == null)
            {
                Debug.LogWarning("PLAYER: Nenhuma l�gica de ataque ativa. N�o � poss�vel atacar. Certifique-se de que a arma tem um prefab de l�gica de ataque.", this);
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

        // L�gica de uso de item equipado (bot�o direito do mouse)
        if (Input.GetButtonDown("Fire2"))
        {
            Debug.Log("PLAYER: Bot�o direito do mouse (Fire2) clicado. Tentando usar item equipado.");
            if (InventorySystem.Instance != null)
            {
                InventorySystem.Instance.UseEquippedItem();
            }
            else
            {
                Debug.LogWarning("PLAYER: InventorySystem.Instance n�o encontrado. N�o � poss�vel usar item.");
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

        Debug.Log("PLAYER: Delay conclu�do. Tentando chamar PerformAttack na l�gica da arma.");

        if (activeWeaponLogic != null)
        {
            activeWeaponLogic.PerformAttack(); // Chama PerformAttack sem par�metros
            Debug.Log("PLAYER: PerformAttack na l�gica da arma chamado com sucesso.");
        }
        else
        {
            Debug.LogError("PLAYER: ERRO CR�TICO: activeWeaponLogic � NULO ap�s o delay. A l�gica da arma foi destru�da ou nunca foi atribu�da corretamente. O ataque n�o ser� executado.", this);
        }

        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        // A l�gica de preview de Gizmo � tratada pelo OnValidate.
        // Aqui, apenas desenha se as refer�ncias estiverem v�lidas.
        if (activeWeaponLogic == null || attackOriginPoint == null)
        {
            Gizmos.color = Color.grey;
            // Desenha uma esfera simples se n�o houver l�gica de arma ativa para indicar o player
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            return;
        }

        // Usa as estat�sticas da arma equipada para desenhar o Gizmo
        float gizmoRange = (currentEquippedWeapon != null) ? currentEquippedWeapon.baseAttackRange : 1.0f; // Fallback para 1.0f

        activeWeaponLogic.DrawAttackGizmos(attackOriginPoint, gizmoRange); // Passa o alcance atual
    }
}