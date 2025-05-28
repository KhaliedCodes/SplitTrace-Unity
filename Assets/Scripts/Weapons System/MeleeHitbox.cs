using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    MeleeWeapon meleeWeapon;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            IDamagable target = other.GetComponent<IDamagable>();

            if (target != null)
            {
                target.TakeDamage(meleeWeapon.damage);
                Debug.Log("Hit " + other.name + " for " + meleeWeapon.damage + " damage.");
            }
        }
    }
}
