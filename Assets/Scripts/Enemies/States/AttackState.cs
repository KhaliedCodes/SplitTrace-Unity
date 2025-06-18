using UnityEngine;

public class AttackState : IEnemyStates
{
    private const int AttackLayerIndex = 1; // Index of the AttackLayer

    public void EnterState(IEnemy enemy)
    {
      //  enemy.NavMeshAgent.isStopped = true;

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
             enemy.Animator.SetBool("attack", false);
        }

        // this is to ensure the enemy stops moving when player in near place according to the NavMesh Stopping Distance
        if (enemy.IsPlayerInAttackRange && enemy.HasLineOfSight())
        {
            float distanceToPlayer = Vector3.Distance(enemy.transform.position, enemy.Player.transform.position);
            if (distanceToPlayer <= enemy.NavMeshAgent.stoppingDistance)
            {
                enemy.Animator.SetFloat("speed", 0f);
            }
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
