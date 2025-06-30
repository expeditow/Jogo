// SwordSwingAttack.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SwordSwingAttack : WeaponAttackLogic
{
    public override void PerformAttack(int damage, float attackRange)
    {
        if (attackOriginPoint == null)
        {
            Debug.LogError("SWORD: Attack Origin Point não está configurado para o ataque de balanço! Não é possível executar.", this);
            return;
        }
        if (audioSource == null)
        {
            Debug.LogWarning("SWORD: AudioSource não está configurado. Sons de ataque não serão tocados.", this);
        }

        Debug.Log("SWORD: Executando ataque de balanço de espada!");

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        ResetHits();

        Vector3 attackStartOrigin = attackOriginPoint.position;
        attackStartOrigin += attackOriginPoint.up * sweepHeightOffset;
        Vector3 initialSweepDirection = attackOriginPoint.forward;

        int numSweepSteps = 5;

        for (int i = 0; i < numSweepSteps; i++)
        {
            float currentAngle = Mathf.Lerp(-sweepAngle / 2f, sweepAngle / 2f, (float)i / (numSweepSteps - 1));
            Quaternion sweepRotation = Quaternion.AngleAxis(currentAngle, attackOriginPoint.up);
            Vector3 currentSweepDirection = sweepRotation * initialSweepDirection;

            Debug.DrawRay(attackStartOrigin, currentSweepDirection * attackRange, Color.cyan, 0.1f);

            RaycastHit hit;
            if (Physics.SphereCast(attackStartOrigin, sweepRadius, currentSweepDirection, out hit, attackRange))
            {
                Debug.Log($"SWORD: SphereCast atingiu um objeto: {hit.collider.name}.");
                ApplyHit(hit.collider, damage);
            }
            else
            {
                Debug.Log($"SWORD: SphereCast não atingiu nada neste passo {i}.");
            }
        }
    }

    public override void DrawAttackGizmos(Transform originPoint, float attackRange)
    {
        Debug.Log($"SWORD: DrawAttackGizmos chamado para {this.name}. Origin: {originPoint?.name ?? "Nulo"}, Range: {attackRange}.");

        base.DrawAttackGizmos(originPoint, attackRange);

        Debug.Log("SWORD: Gizmo desenhado com sucesso.");
    }
}