using UnityEngine;

public class PrepScreamState : IEnemyStates
{
    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.Play("ChargeScream");
        enemy.NavMeshAgent.isStopped = true;
        enemy.ChangeState(new ScreamState());
    }

    public void UpdateState(IEnemy enemy)
    {
    
    }

    public void ExitState(IEnemy enemy)
    {
     
    }
}
