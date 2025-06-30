// CylinderMeleeAttack.cs
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CylinderMeleeAttack : MonoBehaviour
{
    [Header("Equipamento")]
    [Tooltip("Arraste o Asset de WeaponStats da arma atualmente equipada aqui. Define as estatísticas e a lógica de ataque.")]
    public WeaponStats currentEquippedWeapon;

    [Header("Pontos de Referência")]
    [Tooltip("Objeto vazio que define a origem e direção do seu ataque (ex: na frente da mira do cilindro).")]
    public Transform attackOriginPoint;

    [Header("Referências Visuais/Sonoras Comuns do Jogador")]
    [Tooltip("Prefab de efeito de partículas ou visual genérico para o impacto do ataque. Usado pela lógica da arma.")]
    public GameObject hitEffectPrefab;
    [Tooltip("Som genérico reproduzido no momento que o ataque é ativado. Usado pela lógica da arma.")]
    public AudioClip defaultAttackSound;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private AudioSource audioSource;
    // Referência à instância da lógica de ataque da arma atualmente ativa.
    private WeaponAttackLogic activeWeaponLogic;

    // Referência ao script de movimento do jogador para gerenciar estamina
    private PlayerMovement playerMovement;

    void Awake()
    {
        // Garante que existe um AudioSource no objeto para tocar sons
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Garante que existe uma referência ao PlayerMovement
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PLAYER: Componente PlayerMovement não encontrado no mesmo GameObject que CylinderMeleeAttack! A estamina não será gerenciada.", this);
        }

        // Verifica se o Attack Origin Point está configurado
        if (attackOriginPoint == null)
        {
            Debug.LogError("PLAYER: Attack Origin Point não configurado no Cilindro do Jogador! Ataque pode não funcionar.", this);
            attackOriginPoint = this.transform; // Fallback: usa o próprio transform do jogador
        }

        // Equipa a arma padrão ao iniciar o jogo.
        // É essencial que currentEquippedWeapon não seja nulo e que EquipWeapon seja chamado aqui.
        if (currentEquippedWeapon != null)
        {
            EquipWeapon(currentEquippedWeapon);
        }
        else
        {
            Debug.LogWarning("PLAYER: Nenhuma arma padrão definida no Inspector. O ataque pode não funcionar corretamente no início.", this);
        }
    }

    // OnValidate é chamado no editor (e em play mode quando variáveis são alteradas no Inspector).
    // Usaremos para preview de Gizmos no editor.
    void OnValidate()
    {
        // A lógica de OnValidate só deve rodar no Editor.
        // A verificação `!Application.isPlaying` já está correta.
        // Adicionar `this == null` é uma segurança extra.
        if (Application.isPlaying || this == null)
        {
            return;
        }

        // Agendamos a atualização para depois do ciclo de atualização do Inspector.
        // Isso é a forma correta e segura de manipular objetos em resposta ao OnValidate.
        EditorApplication.delayCall += () =>
        {
            // Verificação de segurança: o objeto pode ter sido destruído
            // pelo usuário no editor enquanto o delayCall esperava para executar.
            if (this == null) return;

            // --- 1. LÓGICA DE LIMPEZA (Cleanup) ---
            // Primeiro, destruímos qualquer objeto de preview antigo.
            // É seguro usar DestroyImmediate aqui porque estamos dentro do delayCall.
            WeaponAttackLogic oldLogic = GetComponentInChildren<WeaponAttackLogic>();
            if (oldLogic != null && oldLogic.gameObject.name == "ActiveWeaponLogic_EditorPreview")
            {
                DestroyImmediate(oldLogic.gameObject);
            }

            // Limpa a referência local
            activeWeaponLogic = null;


            // --- 2. LÓGICA DE CRIAÇÃO (Instantiation) ---
            // Agora, criamos o novo objeto de preview se uma arma válida estiver selecionada.
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
                    if (source == null)
                    {
                        source = gameObject.AddComponent<AudioSource>();
                    }

                    // Inicializa a lógica para o preview do Gizmo.
                    activeWeaponLogic.Initialize(attackOriginPoint, hitEffectPrefab, defaultAttackSound, source);
                }
            }
        };
    }


    void Update()
    {
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

            if (currentEquippedWeapon == null)
            {
                Debug.LogWarning("PLAYER: Nenhuma arma equipada. Não é possível atacar.", this);
                return;
            }
            if (activeWeaponLogic == null)
            {
                Debug.LogWarning("PLAYER: Lógica de ataque ativa é NULA. Arma pode não ter logicPrefab configurado ou falha na inicialização.", this);
                return;
            }

            if (playerMovement != null && playerMovement.GetCurrentStamina() < currentEquippedWeapon.attackStaminaCost)
            {
                Debug.Log("PLAYER: Estamina insuficiente para atacar. Estamina atual: " + playerMovement.GetCurrentStamina() + ", Custo: " + currentEquippedWeapon.attackStaminaCost);
                return;
            }

            Debug.Log("PLAYER: Iniciando coroutine PerformMeleeAttackWithDelay().");
            StartCoroutine(PerformMeleeAttackWithDelay());
            nextAttackTime = Time.time + currentEquippedWeapon.baseAttackCooldown;
        }
    }

    IEnumerator PerformMeleeAttackWithDelay()
    {
        isAttacking = true;
        Debug.Log("PLAYER: Coroutine PerformMeleeAttackWithDelay() iniciada. Aguardando delay...");

        if (playerMovement != null)
        {
            playerMovement.ConsumeStamina(currentEquippedWeapon.attackStaminaCost);
            Debug.Log($"PLAYER: Estamina consumida: {currentEquippedWeapon.attackStaminaCost}.");
        }

        yield return new WaitForSeconds(currentEquippedWeapon.baseAttackActivationDelay);

        Debug.Log("PLAYER: Delay concluído. Tentando chamar PerformAttack na lógica da arma.");

        if (activeWeaponLogic != null)
        {
            activeWeaponLogic.PerformAttack(currentEquippedWeapon.baseDamage, currentEquippedWeapon.baseAttackRange);
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
        if (!Application.isPlaying)
        {
            // O OnValidate já cuidou da criação e limpeza do activeWeaponLogic para preview.
            // Aqui, apenas garantimos que a referência está limpa se currentEquippedWeapon for nulo,
            // e OnValidate não recriou nada.
            if (currentEquippedWeapon == null && activeWeaponLogic != null)
            {
                // Para OnDrawGizmosSelected, se o objeto já foi marcado com HideAndDontSave,
                // ele pode ter sido destruído internamente ou será logo.
                // A verificação `activeWeaponLogic == null` já lida com isso.
                // Não é estritamente necessário chamar DestroyImmediate aqui novamente.
            }
        }

        if (currentEquippedWeapon == null || attackOriginPoint == null || activeWeaponLogic == null)
        {
            Gizmos.color = Color.grey;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            return;
        }

        activeWeaponLogic.DrawAttackGizmos(attackOriginPoint, currentEquippedWeapon.baseAttackRange);
    }

    public void EquipWeapon(WeaponStats newWeapon)
    {
        if (newWeapon == null)
        {
            Debug.LogWarning("PLAYER: Tentou equipar uma arma nula. Nenhuma mudança feita.", this);
            if (activeWeaponLogic != null)
            {
                Destroy(activeWeaponLogic.gameObject);
                activeWeaponLogic = null;
            }
            currentEquippedWeapon = null;
            return;
        }

        if (activeWeaponLogic != null)
        {
            Debug.Log($"PLAYER: Destruindo lógica de ataque anterior: {activeWeaponLogic.name}.");
            Destroy(activeWeaponLogic.gameObject);
            activeWeaponLogic = null;
        }

        currentEquippedWeapon = newWeapon;
        Debug.Log($"PLAYER: Arma equipada: {newWeapon.name}. Dano: {newWeapon.baseDamage}, Cooldown: {newWeapon.baseAttackCooldown}, Alcance: {newWeapon.baseAttackRange}.");

        if (currentEquippedWeapon.attackLogicPrefab != null)
        {
            GameObject logicGO = Instantiate(currentEquippedWeapon.attackLogicPrefab, transform);
            logicGO.name = "ActiveWeaponLogic_" + newWeapon.name;
            activeWeaponLogic = logicGO.GetComponent<WeaponAttackLogic>();

            if (activeWeaponLogic != null)
            {
                activeWeaponLogic.Initialize(attackOriginPoint, hitEffectPrefab, defaultAttackSound, audioSource);
                Debug.Log($"PLAYER: Lógica '{activeWeaponLogic.name}' instanciada e inicializada com sucesso em EquipWeapon.");
            }
            else
            {
                Debug.LogError($"PLAYER: O Prefab de lógica de ataque '{currentEquippedWeapon.attackLogicPrefab.name}' não contém um componente WeaponAttackLogic! Verifique o prefab.", this);
            }
        }
        else
        {
            Debug.LogError($"PLAYER: O WeaponStats '{newWeapon.name}' não tem um prefab de lógica de ataque configurado. O ataque não funcionará!", this);
        }
    }
}