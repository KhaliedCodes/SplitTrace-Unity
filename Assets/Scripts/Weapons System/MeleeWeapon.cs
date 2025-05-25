using System.Collections;
using StarterAssets;
using UnityEngine;

public class MeleeWeapon : Weapon
{
  public int damage;
  public float attackRange;
  public Collider hitbox;
  public float attackDuration = 0.3f;



    private void Awake()
    {
        weaponType = WeaponType.Melee;
        if (hitbox != null) hitbox.enabled = false;
    }
  
    public override void Use()
    {
        StartCoroutine(PerformAttack());

    }
    IEnumerator PerformAttack()
    {
        Debug.Log("Swinging melee weapon...");
        hitbox.enabled = true;

        yield return new WaitForSeconds(attackDuration);

        hitbox.enabled = false;
        Debug.Log("Attack finished.");
    }
    public override void Reload()
    {
        // Implement reloading logic here
        Debug.Log("Reloading melee weapon is not applicable.");
    }
}
