// AreaAttackLogic.cs
using UnityEngine;

public class AreaAttackLogic : WeaponAttackLogic
{
    [Header("Configura��es Espec�ficas de Ataque em �rea")]
    public float areaRadius = 1.0f;
    public Vector3 areaOffset = Vector3.zero;

    // PerformAttack agora usa os internos 'currentDamage' e 'currentAttackRange'
    public override void PerformAttack() // Par�metros damage, attackRange removidos
    {
        if (attackOriginPoint == null) return;
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound); // Toca som

        ResetHits(); // Limpa acertos de swing anterior

        Vector3 sphereCenter = attackOriginPoint.position + attackOriginPoint.rotation * areaOffset;
        Collider[] hitColliders = Physics.OverlapSphere(sphereCenter, areaRadius);

        foreach (var hitCollider in hitColliders)
        {
            ApplyHit(hitCollider); // Chama ApplyHit sem par�metro de dano
        }
    }

    public override void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        if (originPoint == null) return;
        Gizmos.color = Color.magenta;
        Vector3 gizmoSphereCenter = originPoint.position + originPoint.rotation * areaOffset;
        Gizmos.DrawWireSphere(gizmoSphereCenter, areaRadius);
    }
}