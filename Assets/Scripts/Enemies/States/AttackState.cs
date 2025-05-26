using UnityEngine;

public class AttackState : IEnemyStates
{
    public void EnterState(IEnemy enemy)
    {
        enemy.NavMeshAgent.isStopped = true;
        if (enemy.Player != null)
        {
            enemy.transform.LookAt(enemy.Player.transform);
        }
    }

    public void UpdateState(IEnemy enemy)
    {
        if (enemy.Player == null) return;

        if (!enemy.IsPlayerInAttackRange && enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new DetectionState());
            return;
        }

        if (!enemy.IsPlayerInDetectionRange || !enemy.HasLineOfSight())
        {
            enemy.ChangeState(new IdleState());
            return;
        }

        if (enemy.CanAttack())
        {
            enemy.Animator.SetTrigger("attack");

            // Handle ranged attack if it's a RangedEnemy
            if (enemy is RangedEnemy rangedEnemy)
            {
                rangedEnemy.ShootProjectile();
            }
            // Melee attack would be handled via animation events
            if (enemy is Enemy meleeEnemy)
            {
                meleeEnemy.animator.Play("Attack");
            }
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.NavMeshAgent.isStopped = false;
    }
}