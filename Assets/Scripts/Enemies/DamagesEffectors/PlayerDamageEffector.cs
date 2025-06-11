using UnityEngine;
public class PlayerDamageEffector : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.PlayOneShotAtPosition("Enemy", "Attack", transform.position);
            other.GetComponent<PlayerHealth>().TakeDamage(enemy.attackDamage);
        }
    }
}
