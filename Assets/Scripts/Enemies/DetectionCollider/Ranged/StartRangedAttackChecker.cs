using UnityEngine;

public class StartRangedAttackChecker : MonoBehaviour
{
    [SerializeField] private RangedEnemy _enemy;


    public void Initialize(float range, RangedEnemy enemy)
    {
        _enemy = enemy;
        GetComponent<SphereCollider>().radius = range;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemy.SetPlayerInAttackRange(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _enemy.SetPlayerInAttackRange(false);
        }
    }
}
