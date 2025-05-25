using UnityEngine;

public class RangedWeapon : Weapon
{

    [SerializeField] int damage = 10;
    [SerializeField] int magazineCapacity = 30;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] Bullet bulletPrefab;
    [SerializeField]public int ammoInMagazine = 30;
    [SerializeField]public int totalAmmo = 120;
    float lastFireTime = -999f;

    private void Awake()
    {
        weaponType = WeaponType.Ranged;
    }
    public override void Use()
    {
        if (Time.time < lastFireTime + fireRate) return;
        if (ammoInMagazine > 0)
        {
            Bullet bullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            ammoInMagazine--;
            lastFireTime = Time.time;

            Debug.Log("Fired! Remaining ammo: " + ammoInMagazine);
        }
        else
        {
            Debug.Log("Out of ammo!");
        }
    }

    public override void Reload()
    {
        if (totalAmmo > 0)
        {
            int ammoNeeded = magazineCapacity - ammoInMagazine;
            if (totalAmmo >= ammoNeeded)
            {
                ammoInMagazine += ammoNeeded;
                totalAmmo -= ammoNeeded;
            }
            else
            {
                ammoInMagazine += totalAmmo;
                totalAmmo = 0;
            }
            Debug.Log("Reloaded! Current ammo: " + ammoInMagazine);
        }
        else
        {
            Debug.Log("No more total ammo to reload!");
        }
    }
}