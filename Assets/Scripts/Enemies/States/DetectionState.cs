using UnityEngine;

public class DetectionState : IEnemyStates
{
    private bool isSearching = false;
    private float searchStartTime;
    private readonly float searchDuration = 3f;

    public void EnterState(IEnemy enemy)
    {
        enemy.Animator.SetFloat("speed", 1f);
        enemy.NavMeshAgent.speed = enemy.MoveSpeed * 1.5f;

        if(enemy is RangedEnemy)
        {
            enemy.Animator.SetBool("Detect", true);
        }

        isSearching = false;
        searchStartTime = 0f;
    }

    public void UpdateState(IEnemy enemy)
    {
        // If player left detection zone entirely, return to idle
        if (!enemy.IsPlayerInDetectionRange)
        {
            if (enemy is RangedEnemy)
            {
                enemy.Animator.SetBool("Detect", false);
            }
            enemy.ChangeState(new IdleState());

            return;
        }

        // If player reference is lost, do nothing
        if (enemy.Player == null) return;

        if (enemy.HasLineOfSight() || enemy.IsProvoked)
        {
            // Player visible: pursue
            isSearching = false;

            enemy.LastKnownPlayerPosition = enemy.Player.transform.position;
            enemy.NavMeshAgent.SetDestination(enemy.Player.transform.position);

            if (enemy.IsPlayerInAttackRange && enemy.HasLineOfSight())
            {
                if (enemy is RangedEnemy)
                {
                    enemy.Animator.SetBool("Detect", false);
                    //RangedEnemy renemy = enemy as RangedEnemy;
                    //renemy.LookAtPlayer();
                }
                enemy.Animator.SetBool("Shoot",true);

                enemy.ChangeState(new AttackState());
            }
        }
        else
        {
            // No line of sight
            if (!isSearching)
            {
                isSearching = true;
                searchStartTime = Time.time;

                enemy.NavMeshAgent.SetDestination(enemy.LastKnownPlayerPosition);
            }
            else
            {
                // Still searching for the player
                enemy.NavMeshAgent.SetDestination(enemy.LastKnownPlayerPosition);

                if (Time.time - searchStartTime >= searchDuration)
                {
                    // Search expired: go idle
                    if (enemy is RangedEnemy)
                    {
                        enemy.Animator.SetBool("Detect", false);
                    }
                    enemy.ChangeState(new IdleState());
                }
            }
        }
    }

    public void ExitState(IEnemy enemy)
    {
        enemy.Animator.SetFloat("speed", 1f);
        isSearching = false;
    }
}
