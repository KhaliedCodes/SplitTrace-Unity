using UnityEngine;

public class DetectionState : IEnemyStates
{
    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", true);
        enemy.NavMeshAgent.speed = enemy.MoveSpeed * 1.5f;
    }

    public void UpdateState(IEnemy enemy)
    {
        if (!enemy.IsPlayerInDetectionRange)
        {
            enemy.ChangeState(new IdleState());
            return;
        }

        if (enemy.Player != null)
        {
            enemy.NavMeshAgent.SetDestination(enemy.Player.transform.position);
        }

        if (enemy.IsPlayerInAttackRange)
        {
            enemy.ChangeState(new AttackState());
            return;
        }

        enemy.NavMeshAgent.SetDestination(enemy.Player.transform.position);
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", false);
    }
}