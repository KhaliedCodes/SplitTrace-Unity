using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [SerializeField] float damage = 10f;

    public void init(float damage)
    {
        this.damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            IDamagable target = other.GetComponent<IDamagable>();

            if (target != null)
            {
                target.TakeDamage(damage);
                Debug.Log("Hit " + other.name + " for " + damage + " damage.");

            }
        }
    }
}
