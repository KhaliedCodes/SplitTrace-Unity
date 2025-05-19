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
        if (enemy.Player == null) return;

        if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) > enemy.DetectionRange)
        {
            enemy.ChangeState(new IdleState());
            return;
        }

        enemy.NavMeshAgent.SetDestination(enemy.Player.transform.position);

        if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) < enemy.AttackRange)
        {
            enemy.ChangeState(new AttackState());
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.Animator.SetBool("IsMoving", false);
    }
}