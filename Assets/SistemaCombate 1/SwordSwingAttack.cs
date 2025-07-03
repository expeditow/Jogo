// SwordSwingAttack.cs
using UnityEngine;

public class SwordSwingAttack : WeaponAttackLogic
{
    // PerformAttack agora usa os internos 'currentDamage' e 'currentAttackRange'
    public override void PerformAttack() // Par�metros damage, attackRange removidos
    {
        if (attackOriginPoint == null)
        {
            Debug.LogError("Ponto de origem do ataque n�o configurado!", this);
            return;
        }

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound); // Toca som
        }

        ResetHits(); // Limpa acertos de swing anterior

        // Detecta todos os colliders em uma esfera para otimiza��o inicial
        // Usa 'currentAttackRange' de WeaponAttackLogic
        Collider[] hitColliders = Physics.OverlapSphere(attackOriginPoint.position, currentAttackRange + sweepRadius);

        foreach (var hitCollider in hitColliders)
        {
            // Calcula o �ngulo para ver se o inimigo est� dentro do cone de ataque
            Vector3 directionToTarget = (hitCollider.transform.position - attackOriginPoint.position).normalized;
            float angleToTarget = Vector3.Angle(attackOriginPoint.forward, directionToTarget);

            if (angleToTarget <= sweepAngle / 2) // Usa 'sweepAngle' de WeaponAttackLogic
            {
                // Se estiver dentro do �ngulo, chama a l�gica central para aplicar dano e efeitos
                ApplyHit(hitCollider); // Chama ApplyHit sem par�metro de dano
            }
        }
    }

    public override void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        // Reutiliza a l�gica de desenho da classe-m�e
        base.DrawAttackGizmos(originPoint, attackRange);
    }
}