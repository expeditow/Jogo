// AreaAttackLogic.cs
using UnityEngine;
using System.Collections.Generic;

public class AreaAttackLogic : WeaponAttackLogic
{
    [Header("Configurações Específicas de Ataque em Área")]
    [Tooltip("Raio da esfera para a detecção de ataque em área.")]
    public float areaRadius = 1.0f;
    [Tooltip("Offset (deslocamento) da esfera de área em relação ao Attack Origin Point.")]
    public Vector3 areaOffset = Vector3.zero;

    public override void PerformAttack(int damage, float attackRange)
    {
        if (attackOriginPoint == null)
        {
            Debug.LogError("AREA: Attack Origin Point não está configurado para o ataque em área! Não é possível executar.", this);
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("AREA: AudioSource não está configurado. Sons de ataque não serão tocados.", this);
        }

        Debug.Log("AREA: Executando ataque em área!");

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        ResetHits();

        Vector3 sphereCenter = attackOriginPoint.position + attackOriginPoint.rotation * areaOffset;

        Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, areaRadius);

        foreach (var hitCollider in hitColliders)
        {
            ApplyHit(hitCollider, damage);
        }
    }

    public override void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        Debug.Log($"AREA: DrawAttackGizmos chamado para {this.name}. Origin: {originPoint?.name ?? "Nulo"}, Range: {attackRange}.");

        if (originPoint == null)
        {
            Debug.LogWarning("AREA: Gizmo não desenhado: originPoint é nulo dentro da lógica de ataque de área.");
            return;
        }

        Gizmos.color = Color.magenta;

        Vector3 gizmoSphereCenter = originPoint.position + originPoint.rotation * areaOffset;

        Gizmos.DrawWireSphere(gizmoSphereCenter, areaRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(originPoint.position, 0.1f);
        Debug.Log("AREA: Gizmo desenhado com sucesso.");
    }
}