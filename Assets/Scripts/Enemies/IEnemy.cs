using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public interface IEnemy
{
    //Stats
    float MoveSpeed { get; set; }
    Transform transform { get; }
    float DetectionRange { get; set; }
    float AttackRange { get; set; }
    float AttackCooldown { get; set; }

    //ref
    GameObject Player { get; }
    NavMeshAgent NavMeshAgent { get; }
    Animator Animator { get; }

    //State Checks
    bool IsDead { get; }
    bool IsPlayerInDetectionRange { get; }
    bool IsPlayerInAttackRange { get; }
    Vector3 LastKnownPlayerPosition { get; set; }
    bool CanAttack();
    bool HasLineOfSight();
    //patrol
    List<Transform> Waypoints { get; set; }
    float WaypointStopTime { get; set; }
    int CurrentWaypointIndex { get; set; }

    //Methods
    void Die();
    void ChangeState(IEnemyStates newState);
}
