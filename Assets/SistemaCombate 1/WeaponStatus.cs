// WeaponStats.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponStats", menuName = "Weapon/Melee Weapon Stats")]
public class WeaponStats : ScriptableObject
{
    [Header("Estatísticas da Arma Melee")]
    [Tooltip("Dano base que esta arma causa em um único acerto.")]
    public int baseDamage = 15;
    [Tooltip("Tempo em segundos entre cada ataque com esta arma.")]
    public float baseAttackCooldown = 0.6f;
    [Tooltip("Tempo em segundos de atraso antes do hitbox do ataque ser ativado.")]
    public float baseAttackActivationDelay = 0.25f;
    [Tooltip("Alcance máximo do ataque desta arma em unidades do mundo.")]
    public float baseAttackRange = 1.0f;
    [Tooltip("Custo de estamina para realizar um ataque com esta arma.")]
    public float attackStaminaCost = 10f; // Variável que adicionamos recentemente

    // --- VARIÁVEIS QUE VOCÊ PRECISA GARANTIR QUE ESTÃO AQUI ---
    public LayerMask enemyLayer;     // Camada dos inimigos (se for usada para detecção aqui)
    public GameObject hitEffectPrefab; // Prefab de efeito de acerto (opcional)
    public AudioClip attackSound;    // Som de ataque (opcional)
    // --- FIM DAS VARIÁVEIS A GARANTIR ---

    [Header("Lógica de Ataque")]
    [Tooltip("Prefab do GameObject que contém o script de lógica de ataque (ex: SwordSwingAttack).")]
    public GameObject attackLogicPrefab; // Este prefab será instanciado para executar o ataque
}