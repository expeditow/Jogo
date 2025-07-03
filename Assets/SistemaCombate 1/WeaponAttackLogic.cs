// WeaponAttackLogic.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class WeaponAttackLogic : MonoBehaviour
{
    [Header("Configurações Comuns do Hitbox")]
    [Tooltip("O raio da esfera usada para detectar colisões durante o ataque.")]
    public float sweepRadius = 0.5f;
    [Tooltip("O ângulo total do arco que o golpe varre (ex: 90 graus para um corte lateral).")]
    public float sweepAngle = 90f; // Usado por ataques baseados em cone
    [Tooltip("Ajuste vertical da origem do hitbox em relação ao Attack Origin Point.")]
    public float sweepHeightOffset = 0.0f;

    protected Transform attackOriginPoint;
    protected GameObject hitEffectPrefab;
    protected AudioClip attackSound;
    protected AudioSource audioSource;
    protected HashSet<GameObject> hitEnemiesThisSwing = new HashSet<GameObject>(); // Para evitar múltiplos acertos no mesmo inimigo por swing

    // NOVO: Armazena o dano e alcance reais, passados do CylinderMeleeAttack
    protected int currentDamage;
    protected float currentAttackRange;

    // O método Initialize agora também recebe dano e alcance.
    // Isso permite que a Lógica de Ataque tenha todas as informações necessárias ao realizar seu ataque.
    public void Initialize(Transform originPoint, GameObject hitEffect, AudioClip swingSound, AudioSource source, int damage, float attackRange)
    {
        attackOriginPoint = originPoint;
        hitEffectPrefab = hitEffect;
        attackSound = swingSound;
        audioSource = source;
        currentDamage = damage; // Atribui o valor do dano
        currentAttackRange = attackRange; // Atribui o valor do alcance
    }

    // PerformAttack não recebe mais dano ou attackRange como parâmetros.
    // Ele usa o currentDamage e currentAttackRange armazenados internamente.
    public abstract void PerformAttack();

    public virtual void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        if (originPoint == null) return;
        Gizmos.color = Color.red;
        Vector3 gizmoAttackOrigin = originPoint.position + (originPoint.up * sweepHeightOffset);
        int gizmoNumSegments = 20;
        Vector3 previousPoint = Vector3.zero;

        // Desenha o cone para representação visual
        for (int i = 0; i <= gizmoNumSegments; i++)
        {
            float currentAngle = Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, (float)i / gizmoNumSegments);
            Quaternion sweepRotation = Quaternion.AngleAxis(currentAngle, originPoint.up);
            Vector3 currentDirection = sweepRotation * originPoint.forward;
            Vector3 currentPoint = gizmoAttackOrigin + currentDirection * attackRange;

            // Desenha esfera em intervalos para melhor visibilidade da profundidade
            if (i % 5 == 0 || i == gizmoNumSegments) Gizmos.DrawWireSphere(currentPoint, sweepRadius);

            // Desenha linhas para formar o cone
            if (i > 0) Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        // Desenha linhas da origem até a base do cone para melhor visualização
        Gizmos.DrawLine(gizmoAttackOrigin, gizmoAttackOrigin + (Quaternion.AngleAxis(-sweepAngle / 2f, originPoint.up) * originPoint.forward) * attackRange);
        Gizmos.DrawLine(gizmoAttackOrigin, gizmoAttackOrigin + (Quaternion.AngleAxis(sweepAngle / 2f, originPoint.up) * originPoint.forward) * attackRange);
    }

    // =======================================================================
    // MÉTODO APPLYHIT (Agora usa currentDamage)
    // =======================================================================
    protected void ApplyHit(Collider hitCollider) // Parâmetro 'damage' removido
    {
        // Impede acertos em si mesmo (se a origem faz parte da hierarquia do jogador)
        if (attackOriginPoint != null && hitCollider.transform.root == attackOriginPoint.root) return;
        // Impede múltiplos acertos no mesmo inimigo por swing
        if (hitEnemiesThisSwing.Contains(hitCollider.gameObject)) return;

        // Tenta obter todos os componentes relevantes do objeto atingido
        Health enemyHealth = hitCollider.GetComponent<Health>();
        // Assumindo que MobsPassivos, EnemyAI e InimigoNormal existem e têm um método OnHit(Vector3)
        MobsPassivos mobPassivo = hitCollider.GetComponent<MobsPassivos>();
        EnemyAI enemyAI = hitCollider.GetComponent<EnemyAI>();
        InimigoNormal inimigoNormal = hitCollider.GetComponent<InimigoNormal>();

        // Se o objeto atingido não tem nenhum dos componentes que nos interessam, ignora.
        if (enemyHealth == null && mobPassivo == null && enemyAI == null && inimigoNormal == null)
        {
            return;
        }

        hitEnemiesThisSwing.Add(hitCollider.gameObject);
        Vector3 impactPoint = hitCollider.bounds.center; // Ponto de impacto aproximado

        // Aplica dano se o componente Health estiver presente
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(currentDamage); // Usa currentDamage
            Debug.Log($"[WeaponAttackLogic] {hitCollider.gameObject.name} tomou {currentDamage} de dano.");
        }

        // Chama OnHit para tipos específicos de inimigos, se eles tiverem
        if (mobPassivo != null)
        {
            mobPassivo.OnHit(impactPoint);
        }

        if (enemyAI != null)
        {
            enemyAI.OnHit(impactPoint);
        }

        if (inimigoNormal != null)
        {
            inimigoNormal.OnHit(impactPoint);
        }

        // Instancia efeito de acerto se o prefab for fornecido
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, impactPoint, Quaternion.identity);
        }
    }

    public virtual void ResetHits()
    {
        hitEnemiesThisSwing.Clear();
    }
}