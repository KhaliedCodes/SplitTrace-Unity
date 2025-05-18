using UnityEngine;

public class AttackState : IEnemyStates
{
    public void EnterState(Enemy enemy)
    {
        enemy.navMeshAgent.isStopped = true;
        enemy.transform.LookAt(enemy.Player.transform);
    }

    public void UpdateState(Enemy enemy)
    {
        if (!enemy.IsPlayerInAttackRange && enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new DetectionState()); 
            return;
        }

        if (!enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new IdleState()); 
            return;
        }

        if (enemy.CanAttack())
        {
            enemy.animator.Play("Attack");
        }
    }
    public void ExitState(Enemy enemy) 
    {
        enemy.navMeshAgent.isStopped = false;
    }
}
