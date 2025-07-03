// SwordSwingAttack.cs
using UnityEngine;

public class SwordSwingAttack : WeaponAttackLogic
{
    // PerformAttack agora usa os internos 'currentDamage' e 'currentAttackRange'
    public override void PerformAttack() // Parâmetros damage, attackRange removidos
    {
        if (attackOriginPoint == null)
        {
            Debug.LogError("Ponto de origem do ataque não configurado!", this);
            return;
        }

        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound); // Toca som
        }

        ResetHits(); // Limpa acertos de swing anterior

        // Detecta todos os colliders em uma esfera para otimização inicial
        // Usa 'currentAttackRange' de WeaponAttackLogic
        Collider[] hitColliders = Physics.OverlapSphere(attackOriginPoint.position, currentAttackRange + sweepRadius);

        foreach (var hitCollider in hitColliders)
        {
            // Calcula o ângulo para ver se o inimigo está dentro do cone de ataque
            Vector3 directionToTarget = (hitCollider.transform.position - attackOriginPoint.position).normalized;
            float angleToTarget = Vector3.Angle(attackOriginPoint.forward, directionToTarget);

            if (angleToTarget <= sweepAngle / 2) // Usa 'sweepAngle' de WeaponAttackLogic
            {
                // Se estiver dentro do ângulo, chama a lógica central para aplicar dano e efeitos
                ApplyHit(hitCollider); // Chama ApplyHit sem parâmetro de dano
            }
        }
    }

    public override void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        // Reutiliza a lógica de desenho da classe-mãe
        base.DrawAttackGizmos(originPoint, attackRange);
    }
}