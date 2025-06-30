// AreaAttackLogic.cs
using UnityEngine;
using System.Collections.Generic;

public class AreaAttackLogic : WeaponAttackLogic
{
    [Header("Configura��es Espec�ficas de Ataque em �rea")]
    [Tooltip("Raio da esfera para a detec��o de ataque em �rea.")]
    public float areaRadius = 1.0f;
    [Tooltip("Offset (deslocamento) da esfera de �rea em rela��o ao Attack Origin Point.")]
    public Vector3 areaOffset = Vector3.zero;

    public override void PerformAttack(int damage, float attackRange)
    {
        if (attackOriginPoint == null)
        {
            Debug.LogError("AREA: Attack Origin Point n�o est� configurado para o ataque em �rea! N�o � poss�vel executar.", this);
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("AREA: AudioSource n�o est� configurado. Sons de ataque n�o ser�o tocados.", this);
        }

        Debug.Log("AREA: Executando ataque em �rea!");

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
            Debug.LogWarning("AREA: Gizmo n�o desenhado: originPoint � nulo dentro da l�gica de ataque de �rea.");
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