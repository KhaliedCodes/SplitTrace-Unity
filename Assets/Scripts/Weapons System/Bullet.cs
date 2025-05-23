using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed = 20f;
    [SerializeField] float lifetime = 2f;
    [SerializeField] int damage = 10;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Try to get IDamagable component from the enemy
            if (collision.gameObject.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.TakeDamage(damage);
            }
        }
        if (!collision.gameObject.CompareTag("Pistol"))
        {
            Destroy(gameObject);
        }
    }
}
