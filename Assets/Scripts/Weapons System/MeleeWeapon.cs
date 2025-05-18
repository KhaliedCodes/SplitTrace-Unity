using UnityEngine;

public class MeleeWeapon : Weapon
{
  public int damage;
  public float attackRange;

    private void Awake()
    {
        weaponType = WeaponType.Melee;
    }
    public override void Use()
    {

    }

    public override void Reload()
    {
        // Implement reloading logic here
        Debug.Log("Reloading melee weapon is not applicable.");
    }
}
