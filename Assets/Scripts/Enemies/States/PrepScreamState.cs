using UnityEngine;

public class PrepScreamState : IEnemyStates
{
    public void EnterState(IEnemy enemy)
    {
      
            enemy.NavMeshAgent.isStopped = true;
        if (enemy is Enemy Scream)
        {
            enemy.Animator.Play("ChargeScream");
            AudioManager.Instance.PlayOneShotAtPosition("Enemy", "PreScream", Scream.transform.position);
            Scream.StartScreamAfterDelay(2f);

        }
        else
        {
            enemy.ChangeState(new IdleState());
        }
    }

    public void UpdateState(IEnemy enemy)
    {
    
    }

    public void ExitState(IEnemy enemy)
    {
     
    }
}
