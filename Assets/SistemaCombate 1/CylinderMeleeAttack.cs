// CylinderMeleeAttack.cs
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CylinderMeleeAttack : MonoBehaviour
{
    [Header("Equipamento")]
    [Tooltip("Arraste o Asset de WeaponStats da arma atualmente equipada aqui. Define as estat�sticas e a l�gica de ataque.")]
    public WeaponStats currentEquippedWeapon;

    [Header("Pontos de Refer�ncia")]
    [Tooltip("Objeto vazio que define a origem e dire��o do seu ataque (ex: na frente da mira do cilindro).")]
    public Transform attackOriginPoint;

    [Header("Refer�ncias Visuais/Sonoras Comuns do Jogador")]
    [Tooltip("Prefab de efeito de part�culas ou visual gen�rico para o impacto do ataque. Usado pela l�gica da arma.")]
    public GameObject hitEffectPrefab;
    [Tooltip("Som gen�rico reproduzido no momento que o ataque � ativado. Usado pela l�gica da arma.")]
    public AudioClip defaultAttackSound;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    private AudioSource audioSource;
    // Refer�ncia � inst�ncia da l�gica de ataque da arma atualmente ativa.
    private WeaponAttackLogic activeWeaponLogic;

    // Refer�ncia ao script de movimento do jogador para gerenciar estamina
    private PlayerMovement playerMovement;

    void Awake()
    {
        // Garante que existe um AudioSource no objeto para tocar sons
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Garante que existe uma refer�ncia ao PlayerMovement
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PLAYER: Componente PlayerMovement n�o encontrado no mesmo GameObject que CylinderMeleeAttack! A estamina n�o ser� gerenciada.", this);
        }

        // Verifica se o Attack Origin Point est� configurado
        if (attackOriginPoint == null)
        {
            Debug.LogError("PLAYER: Attack Origin Point n�o configurado no Cilindro do Jogador! Ataque pode n�o funcionar.", this);
            attackOriginPoint = this.transform; // Fallback: usa o pr�prio transform do jogador
        }

        // Equipa a arma padr�o ao iniciar o jogo.
        // � essencial que currentEquippedWeapon n�o seja nulo e que EquipWeapon seja chamado aqui.
        if (currentEquippedWeapon != null)
        {
            EquipWeapon(currentEquippedWeapon);
        }
        else
        {
            Debug.LogWarning("PLAYER: Nenhuma arma padr�o definida no Inspector. O ataque pode n�o funcionar corretamente no in�cio.", this);
        }
    }

    // OnValidate � chamado no editor (e em play mode quando vari�veis s�o alteradas no Inspector).
    // Usaremos para preview de Gizmos no editor.
    void OnValidate()
    {
        // A l�gica de OnValidate s� deve rodar no Editor.
        // A verifica��o `!Application.isPlaying` j� est� correta.
        // Adicionar `this == null` � uma seguran�a extra.
        if (Application.isPlaying || this == null)
        {
            return;
        }

        // Agendamos a atualiza��o para depois do ciclo de atualiza��o do Inspector.
        // Isso � a forma correta e segura de manipular objetos em resposta ao OnValidate.
        EditorApplication.delayCall += () =>
        {
            // Verifica��o de seguran�a: o objeto pode ter sido destru�do
            // pelo usu�rio no editor enquanto o delayCall esperava para executar.
            if (this == null) return;

            // --- 1. L�GICA DE LIMPEZA (Cleanup) ---
            // Primeiro, destru�mos qualquer objeto de preview antigo.
            // � seguro usar DestroyImmediate aqui porque estamos dentro do delayCall.
            WeaponAttackLogic oldLogic = GetComponentInChildren<WeaponAttackLogic>();
            if (oldLogic != null && oldLogic.gameObject.name == "ActiveWeaponLogic_EditorPreview")
            {
                DestroyImmediate(oldLogic.gameObject);
            }

            // Limpa a refer�ncia local
            activeWeaponLogic = null;


            // --- 2. L�GICA DE CRIA��O (Instantiation) ---
            // Agora, criamos o novo objeto de preview se uma arma v�lida estiver selecionada.
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
                    if (source == null)
                    {
                        source = gameObject.AddComponent<AudioSource>();
                    }

                    // Inicializa a l�gica para o preview do Gizmo.
                    activeWeaponLogic.Initialize(attackOriginPoint, hitEffectPrefab, defaultAttackSound, source);
                }
            }
        };
    }


    void Update()
    {
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

            if (currentEquippedWeapon == null)
            {
                Debug.LogWarning("PLAYER: Nenhuma arma equipada. N�o � poss�vel atacar.", this);
                return;
            }
            if (activeWeaponLogic == null)
            {
                Debug.LogWarning("PLAYER: L�gica de ataque ativa � NULA. Arma pode n�o ter logicPrefab configurado ou falha na inicializa��o.", this);
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

        Debug.Log("PLAYER: Delay conclu�do. Tentando chamar PerformAttack na l�gica da arma.");

        if (activeWeaponLogic != null)
        {
            activeWeaponLogic.PerformAttack(currentEquippedWeapon.baseDamage, currentEquippedWeapon.baseAttackRange);
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
        if (!Application.isPlaying)
        {
            // O OnValidate j� cuidou da cria��o e limpeza do activeWeaponLogic para preview.
            // Aqui, apenas garantimos que a refer�ncia est� limpa se currentEquippedWeapon for nulo,
            // e OnValidate n�o recriou nada.
            if (currentEquippedWeapon == null && activeWeaponLogic != null)
            {
                // Para OnDrawGizmosSelected, se o objeto j� foi marcado com HideAndDontSave,
                // ele pode ter sido destru�do internamente ou ser� logo.
                // A verifica��o `activeWeaponLogic == null` j� lida com isso.
                // N�o � estritamente necess�rio chamar DestroyImmediate aqui novamente.
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
            Debug.LogWarning("PLAYER: Tentou equipar uma arma nula. Nenhuma mudan�a feita.", this);
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
            Debug.Log($"PLAYER: Destruindo l�gica de ataque anterior: {activeWeaponLogic.name}.");
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
                Debug.Log($"PLAYER: L�gica '{activeWeaponLogic.name}' instanciada e inicializada com sucesso em EquipWeapon.");
            }
            else
            {
                Debug.LogError($"PLAYER: O Prefab de l�gica de ataque '{currentEquippedWeapon.attackLogicPrefab.name}' n�o cont�m um componente WeaponAttackLogic! Verifique o prefab.", this);
            }
        }
        else
        {
            Debug.LogError($"PLAYER: O WeaponStats '{newWeapon.name}' n�o tem um prefab de l�gica de ataque configurado. O ataque n�o funcionar�!", this);
        }
    }
}