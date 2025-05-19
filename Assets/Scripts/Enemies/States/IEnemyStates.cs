using UnityEngine;

public interface IEnemyStates
{
    public void EnterState(IEnemy enemy);
    public void UpdateState(IEnemy enemy);
    public void ExitState(IEnemy enemy);
}
