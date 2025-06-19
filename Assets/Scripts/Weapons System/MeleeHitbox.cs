using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    int Damage;

    private void Start()
    {
        Damage = GetComponentInParent<MeleeWeapon>().damage; 
           
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemyHealth = other.GetComponent<IDamagable>();

            if (enemyHealth == null) return;

            enemyHealth.TakeDamage(Damage);
        }
    }
}
