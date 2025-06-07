using System.Collections;
using UnityEngine;

public enum RangedType
{
    None = 0,
    Pistol = 1,
    Rifle = 2,
}
public class RangedWeapon : Weapon
{

    [SerializeField] public int damage = 10;
    [SerializeField] public int ammoInMagazine = 30;
    [SerializeField] public int totalAmmo = 120;
    [SerializeField] public int rangedType;
    [SerializeField] public Transform firePoint;
    [SerializeField] int magazineCapacity = 30;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] GameObject bulletHoleEffect;
    [SerializeField] LayerMask aimLayerMask;
    [SerializeField] Bullet bulletPrefab;


    private float lastFireTime = -999f;

    private void Awake()
    {
        weaponType = WeaponType.Ranged;

    }

    public override void Use(Vector3 aimPoint)
    {
        if (Time.time < lastFireTime + fireRate) return;

        if (ammoInMagazine > 0)
        {
            Vector3 direction = (aimPoint - firePoint.position).normalized;
            if (!WeaponManager.Instance.IsAiming)
            {
                Transform playerTransform = WeaponManager.Instance.transform;

                Vector3 playerToAim = aimPoint - playerTransform.position;
                playerToAim.y = 0f; 

                Quaternion targetRotation = Quaternion.LookRotation(playerToAim);
                playerTransform.rotation = targetRotation.normalized;

            }
            GameObject impact = Instantiate(bulletHoleEffect, aimPoint, Quaternion.LookRotation(aimPoint));
            Destroy(impact, 2f);

            ammoInMagazine--;
                lastFireTime = Time.time;
            
                Bullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(direction, Vector3.up));
            Debug.Log("Fired! Remaining ammo: " + ammoInMagazine);
            StartCoroutine(ActivateMuzzleFlash());
            AudioManager.Instance.PlayAudioClip("Weapons", $"{weaponName}", false);
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
    public void UpdateAmmoNumber(int amount)
    {
        totalAmmo += amount;
    }

    IEnumerator ActivateMuzzleFlash()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        muzzleFlash.SetActive(false);

    }
 


}
