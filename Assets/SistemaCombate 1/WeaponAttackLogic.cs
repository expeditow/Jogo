// WeaponAttackLogic.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class WeaponAttackLogic : MonoBehaviour
{
    [Header("Configurações Comuns do Hitbox")]
    [Tooltip("O raio da esfera usada para detectar colisões durante o ataque.")]
    public float sweepRadius = 0.5f;
    [Tooltip("O ângulo total do arco que o golpe varre (ex: 90 graus para um corte lateral).")]
    public float sweepAngle = 90f;
    [Tooltip("Ajuste vertical da origem do hitbox em relação ao Attack Origin Point.")]
    public float sweepHeightOffset = 0.0f;

    protected Transform attackOriginPoint;
    protected GameObject hitEffectPrefab;
    protected AudioClip attackSound;
    protected AudioSource audioSource;

    protected HashSet<GameObject> hitEnemiesThisSwing = new HashSet<GameObject>();

    public void Initialize(Transform originPoint, GameObject hitEffect, AudioClip swingSound, AudioSource source)
    {
        attackOriginPoint = originPoint;
        hitEffectPrefab = hitEffect;
        attackSound = swingSound;
        audioSource = source;
        Debug.Log($"LOGIC: Lógica de ataque {this.name} inicializada. Origin: {attackOriginPoint?.name ?? "Nulo"}, AudioSource: {(audioSource != null ? "OK" : "NULO")}.");
    }

    public abstract void PerformAttack(int damage, float attackRange);

    public virtual void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        if (originPoint == null)
        {
            Debug.LogWarning("LOGIC: Gizmo não desenhado: originPoint é nulo dentro da lógica de ataque.");
            return;
        }

        Gizmos.color = Color.red;

        Vector3 gizmoAttackOrigin = originPoint.position;
        gizmoAttackOrigin += originPoint.up * sweepHeightOffset;
        Vector3 gizmoInitialDirection = originPoint.forward;

        int gizmoNumSegments = 20;
        Vector3 previousPoint = Vector3.zero;

        for (int i = 0; i <= gizmoNumSegments; i++)
        {
            float currentAngle = Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, (float)i / gizmoNumSegments);
            Quaternion sweepRotation = Quaternion.AngleAxis(currentAngle, originPoint.up);
            Vector3 currentDirection = sweepRotation * gizmoInitialDirection;
            Vector3 currentPoint = gizmoAttackOrigin + currentDirection * attackRange;

            if (i % 5 == 0 || i == gizmoNumSegments)
            {
                Gizmos.DrawWireSphere(currentPoint, sweepRadius);
            }

            if (i > 0)
            {
                Gizmos.DrawLine(previousPoint, currentPoint);
            }
            previousPoint = currentPoint;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(gizmoAttackOrigin, 0.1f);
        Debug.Log("LOGIC: Gizmo desenhado com sucesso.");
    }

    protected void ApplyHit(Collider hitCollider, int damage)
    {
        Debug.Log($"LOGIC: ApplyHit chamado para: {hitCollider.name}.");

        // Evita que o ataque acerte o próprio jogador
        if (attackOriginPoint != null && hitCollider.gameObject == attackOriginPoint.root.gameObject)
        {
            Debug.Log($"LOGIC: ApplyHit ignorado: acerto no próprio jogador ({hitCollider.name}).");
            return;
        }

        if (hitEnemiesThisSwing.Contains(hitCollider.gameObject))
        {
            Debug.Log($"LOGIC: ApplyHit ignorado: {hitCollider.name} já foi atingido neste golpe.");
            return;
        }

        Health enemyHealth = hitCollider.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
            Debug.Log($"LOGIC: Componente Health encontrado em {hitCollider.name}. Aplicando dano: {damage}.");

            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitCollider.transform.position, Quaternion.LookRotation(hitCollider.transform.forward));
            }
            hitEnemiesThisSwing.Add(hitCollider.gameObject);
        }
        else
        {
            Debug.LogWarning($"LOGIC: Objeto {hitCollider.name} atingido, mas NÃO tem componente Health! Verifique o inimigo.", this);
        }
    }

    public virtual void ResetHits()
    {
        hitEnemiesThisSwing.Clear();
    }
}