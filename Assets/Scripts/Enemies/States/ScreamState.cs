using UnityEngine;

public class ScreamState : IEnemyStates
{
    private float _timer;
    private bool _screamed;

    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.Play("Scream");
        enemy.NavMeshAgent.isStopped = true;

        if (enemy is Enemy e)
        {
            e.StunArea.gameObject.SetActive(true);
        }

        _timer = 2f;
        _screamed = false;
    }

    public void UpdateState(IEnemy enemy)
    {
        _timer -= Time.deltaTime;

        if (!_screamed && _timer <= 0)
        {
            _screamed = true;
            enemy.ChangeState(new DetectionState());
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.NavMeshAgent.isStopped = false;

        if (enemy is Enemy e)
        {
            e.StunArea.gameObject.SetActive(false);
        }
    }
}
