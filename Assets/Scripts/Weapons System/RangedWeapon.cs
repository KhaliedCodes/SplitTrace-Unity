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

    [SerializeField] int magazineCapacity = 30;
    [SerializeField] float fireRate = 0.5f;
    [SerializeField] Bullet bulletPrefab;
    [SerializeField] public int damage = 10;
    [SerializeField] public int ammoInMagazine = 30;
    [SerializeField] public int totalAmmo = 120;
    [SerializeField] public Transform firePoint;
    [SerializeField] GameObject muzzleFlash;
    [SerializeField] GameObject bulletHoleEffect;
    [SerializeField] LayerMask aimLayerMask;
    [SerializeField] public int rangedType;

   
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
            Vector3 aimPoint = GetAimPoint();
            Vector3 direction = (aimPoint - firePoint.position).normalized;
            //firePoint.forward = direction;

            if (!WeaponManager.Instance.IsAiming)
            {
                Transform playerTransform = WeaponManager.Instance.transform;

                Vector3 playerToAim = aimPoint - playerTransform.position;
                playerToAim.y = 0f; 

                Quaternion targetRotation = Quaternion.LookRotation(playerToAim);
                playerTransform.rotation = targetRotation.normalized;

            }

           
            ammoInMagazine--;
            lastFireTime = Time.time;
            //HitImpact();
            Bullet bullet = Instantiate(bulletPrefab, firePoint.position ,Quaternion.LookRotation(direction , Vector3.up));
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
    private Vector3 GetAimPoint()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            //if (aimTarget != null)
            //    aimTarget.position = hit.point;

            return hit.point;
        }

        return ray.origin + ray.direction * 100f;
    }

    //private void HitImpact()
    //{
    //    RaycastHit hit;
    //    if (!Physics.Raycast(firePoint.position, firePoint.forward, out hit, 100f)) return;
    //    GameObject impact = Instantiate(bulletHoleEffect, hit.point, Quaternion.LookRotation(hit.normal));
    //    Destroy(impact, 2f);
    //}

}
 