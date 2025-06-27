using System.Collections;
using StarterAssets;
using UnityEngine;

public enum MeleeType
{
    None = 0,
    Axe = 1,
    Sword = 2,
}
public class MeleeWeapon : Weapon
{
    [SerializeField] float damage;
    [SerializeField] Collider hitbox;
    [SerializeField] public float attackDuration = 0.3f;
    [SerializeField] public int meleeType;
    [SerializeField] MeleeHitbox meleeHitbox;



    private void Awake()
    {
        weaponType = WeaponType.Melee;
        if (hitbox != null) hitbox.enabled = false;  
    }


    public override void Use(Vector3 a)
    {
        StartCoroutine(PerformAttack());

    }
    IEnumerator PerformAttack()
    {

        hitbox.enabled = true;
        meleeHitbox.init(damage);
        yield return new WaitForSeconds(attackDuration);

        hitbox.enabled = false;
    }
    public override void Reload()
    {
        // Implement reloading logic here
        Debug.Log("Reloading melee weapon is not applicable.");
    }
}
