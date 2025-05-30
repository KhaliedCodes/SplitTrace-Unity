using UnityEngine;

public class EnemyBulletEffector : MonoBehaviour
{


    [SerializeField] public float DamageAmount;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>().TakeDamage(DamageAmount);
            Destroy(gameObject);
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
