using UnityEngine;

public class DeathState : IEnemyStates
{
    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.Play("Die");
        enemy.NavMeshAgent.speed = 0;
        enemy.NavMeshAgent.isStopped = true;
        GameObject.Destroy(enemy.transform.gameObject, 2f);
    }
    public void UpdateState(IEnemy enemy)
    {

    }
    public void ExitState(IEnemy enemy)
    {

    }
}
