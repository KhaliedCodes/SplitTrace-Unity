using UnityEngine;

public enum BulletType
{
    Normal,
    Stunning
}

public class EnemyBulletEffector : MonoBehaviour
{
    [SerializeField] public float DamageAmount;
    [SerializeField] private BulletType bulletType;
    [SerializeField] private float stunDuration = 2f; // Duration of stun in seconds

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<CustomThridPersonController>();
            var playerHealth = other.GetComponent<PlayerHealth>();

            if (controller == null || playerHealth == null) return;

            // Normal bullet
            if (bulletType == BulletType.Normal)
            {
                playerHealth.TakeDamage(DamageAmount);
                Destroy(gameObject);
            }
            // Stunning bullet
            else if (bulletType == BulletType.Stunning)
            {
                controller.Stun(stunDuration);
                Destroy(gameObject);
            }
        }

        if (!other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
