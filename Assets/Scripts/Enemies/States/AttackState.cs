using UnityEngine;

public class AttackState : IEnemyStates
{
    private const int AttackLayerIndex = 1; // Index of the AttackLayer

    public void EnterState(IEnemy enemy)
    {
        enemy.NavMeshAgent.isStopped = true;

        if (enemy.Player != null)
        {
            //enemy.Animator.SetFloat("speed", 0f);
            enemy.transform.LookAt(enemy.Player.transform);
        }
    }

    public void UpdateState(IEnemy enemy)
    {
        if (enemy.Player == null) return;

        if (!enemy.IsPlayerInAttackRange && enemy.IsPlayerInDetectionRange)
        {
           // ResetAttackLayerWeight(enemy);
            enemy.ChangeState(new DetectionState());
            return;
        }

        if (!enemy.IsPlayerInDetectionRange || !enemy.HasLineOfSight())
        {
          //  ResetAttackLayerWeight(enemy);
            enemy.ChangeState(new IdleState());
            return;
        }

        if (enemy.CanAttack())
        {
            enemy.Animator.SetBool("attack", true);

            // Set AttackLayer weight to 1 to enable the default Attack animation
            enemy.Animator.SetLayerWeight(AttackLayerIndex, 1f);

            if (enemy is RangedEnemy rangedEnemy)
            {
                rangedEnemy.ShootProjectile();
            }
        }
        else
        {
            // Reset the layer weight if not attacking
           // ResetAttackLayerWeight(enemy);
             enemy.Animator.SetBool("attack", false);
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.NavMeshAgent.isStopped = false;
        enemy.Animator.SetBool("attack", false);
        //ResetAttackLayerWeight(enemy);
    }

    private void ResetAttackLayerWeight(IEnemy enemy)
    {
        enemy.Animator.SetLayerWeight(AttackLayerIndex, 0f);
    }
}
