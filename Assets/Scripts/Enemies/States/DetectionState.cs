using UnityEngine;

public class DetectionState : IEnemyStates
{
    public void EnterState(Enemy enemy)
    {
        enemy.animator.Play("Move");
        enemy.navMeshAgent.speed = enemy.moveSpeed * 1.5f;
    }

    public void UpdateState(Enemy enemy)
    {
        if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) > enemy.detectionRange)
        {
            enemy.ChangeState(new IdleState());
            return;
        }

        // Continue chasing if player is still in range
        enemy.navMeshAgent.SetDestination(enemy.Player.transform.position);

        // Switch to attack if close enough
        if (Vector3.Distance(enemy.transform.position, enemy.Player.transform.position) < enemy.StartAttakingRange)
        {
            enemy.ChangeState(new AttackState());
        }
    }
    public void ExitState(Enemy enemy) { }
}
