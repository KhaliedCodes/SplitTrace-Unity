using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] float speed = 20f;
    [SerializeField] float lifetime = 2f;
    [SerializeField] float damage = 10f;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }



    private void OnTriggerEnter(Collider other)
    {
        IDamagable target = other.GetComponent<IDamagable>();

        if (target != null)
        {

            if (other.CompareTag("Enemy"))
            {
                target.TakeDamage(damage);
            }
        }
        Destroy(gameObject);
    }
}
