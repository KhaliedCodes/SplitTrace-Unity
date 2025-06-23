using System.Collections;
using StarterAssets;
using UnityEngine;

public enum MeleeType
{
    None = 0,
    Sword = 1,
    Axe = 2,
}
public class MeleeWeapon : Weapon
{
    [SerializeField] float damage;
    [SerializeField] float attackRange;
    [SerializeField] Collider hitbox;
    [SerializeField] public float attackDuration = 0.3f;
    [SerializeField] public int meleeType;
    [SerializeField] MeleeHitbox meleeHitbox;



    private void Awake()
    {
        weaponType = WeaponType.Melee;
        if (hitbox != null) hitbox.enabled = false;
    }
    private void Start()
    {

    }

    public override void Use(Vector3 a)
    {
        StartCoroutine(PerformAttack());

    }
    IEnumerator PerformAttack()
    {
        Debug.Log("Swinging melee weapon...");
        AudioManager.Instance.PlayAudioClip("Weapons", $"{weaponName}", false);
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
