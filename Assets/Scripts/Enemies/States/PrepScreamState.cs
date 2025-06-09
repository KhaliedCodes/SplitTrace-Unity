using UnityEngine;

public class PrepScreamState : IEnemyStates
{
    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.Play("ChargeScream");
        enemy.NavMeshAgent.isStopped = true;
     if (enemy is Enemy Scream)
        {
            Scream.StartScreamAfterDelay(2f);
        }
    }

    public void UpdateState(IEnemy enemy)
    {
    
    }

    public void ExitState(IEnemy enemy)
    {
     
    }
}
