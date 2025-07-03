// WeaponStats.cs
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponStats", menuName = "Weapon/Melee Weapon Stats")]
public class WeaponStats : ScriptableObject
{
    [Header("Estat�sticas da Arma Melee")]
    [Tooltip("Dano base que esta arma causa em um �nico acerto.")]
    public int baseDamage = 15;
    [Tooltip("Tempo em segundos entre cada ataque com esta arma.")]
    public float baseAttackCooldown = 0.6f;
    [Tooltip("Tempo em segundos de atraso antes do hitbox do ataque ser ativado.")]
    public float baseAttackActivationDelay = 0.25f;
    [Tooltip("Alcance m�ximo do ataque desta arma em unidades do mundo.")]
    public float baseAttackRange = 1.0f;
    [Tooltip("Custo de estamina para realizar um ataque com esta arma.")]
    public float attackStaminaCost = 10f; // Vari�vel que adicionamos recentemente

    // --- VARI�VEIS QUE VOC� PRECISA GARANTIR QUE EST�O AQUI ---
    public LayerMask enemyLayer;     // Camada dos inimigos (se for usada para detec��o aqui)
    public GameObject hitEffectPrefab; // Prefab de efeito de acerto (opcional)
    public AudioClip attackSound;    // Som de ataque (opcional)
    // --- FIM DAS VARI�VEIS A GARANTIR ---

    [Header("L�gica de Ataque")]
    [Tooltip("Prefab do GameObject que cont�m o script de l�gica de ataque (ex: SwordSwingAttack).")]
    public GameObject attackLogicPrefab; // Este prefab ser� instanciado para executar o ataque
}