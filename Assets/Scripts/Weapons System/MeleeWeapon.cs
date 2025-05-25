using System.Collections;
using StarterAssets;
using UnityEngine;

public class MeleeWeapon : Weapon
{
  public int damage;
  public float attackRange;
  public Collider hitbox;
  public float attackDuration = 0.3f;
    ThirdPersonController player;

    private void Awake()
    {
        weaponType = WeaponType.Melee;
        if (hitbox != null) hitbox.enabled = false;
    }
    private void Start()
    {
        player = FindObjectOfType<ThirdPersonController>();
        if (player == null)
        {
            Debug.LogError("ThirdPersonController not found in the scene.");
        }
    }
    public override void Use()
    {
        StartCoroutine(PerformAttack());

    }
    IEnumerator PerformAttack()
    {
        Debug.Log("Swinging melee weapon...");
        hitbox.enabled = true;

       player.SetAttackAnimation(true); // Assuming you have an attack animation in your controller

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
