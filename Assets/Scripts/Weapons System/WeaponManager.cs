using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Setup")]
    [SerializeField] Transform weaponHolder;
    [SerializeField] float interactRange = 2f;
    [SerializeField] LayerMask pickupLayer;
    [SerializeField] TextMeshProUGUI ammoText;

    [Header("UI Icons")]
    [SerializeField] Image rangedIconUI;
    [SerializeField] Image meleeIconUI;

    [Header("Cinemachine Cameras")]
    [SerializeField] CinemachineVirtualCamera followCamera;
    [SerializeField] CinemachineVirtualCamera aimCamera;

    [Header("Aiming UI")]
    [SerializeField] GameObject defultCrosshairUI;
    [SerializeField] GameObject aimCrosshairUI;

    //[SerializeField] Transform aimTarget;
    //[SerializeField] Transform projectileSpawnPoint;


    private Weapon currentWeapon;
    private RangedWeapon rangedWeapon;
    private MeleeWeapon meleeWeapon;
    private WeaponsInputSystem weaponInputs;
    private PlayerAnimations playerAnimations;
    private float lastFireTime = -999f;
    private int totalAmmo;

    

    private void Awake()
    {
        
        weaponInputs = new WeaponsInputSystem();
        weaponInputs.Enable();
        weaponInputs.WeaponsActions.Reload.performed += OnReload;
        weaponInputs.WeaponsActions.Shoot.performed += OnShoot;
        weaponInputs.WeaponsActions.SwitchWeapon.performed += OnSwitchWeapon;
        weaponInputs.WeaponsActions.Unequip.performed += OnUnequip;
        weaponInputs.WeaponsActions.Drop.performed += OnDrop;
        weaponInputs.WeaponsActions.Pickup.performed += OnPickup;
        weaponInputs.WeaponsActions.Aim.performed += ctx => SetAim(true); 
        weaponInputs.WeaponsActions.Aim.canceled += ctx => SetAim(false);
    }

    private void Start()
    {
        rangedIconUI.enabled = false;
        meleeIconUI.enabled = false;
        ammoText.enabled = false;
        playerAnimations = GetComponent<PlayerAnimations>();
    }

    private void Update()
    {
        AmmoUI();

    }

    private void OnReload(InputAction.CallbackContext context)
    {
        currentWeapon?.Reload();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (!currentWeapon) return;
        
        if (currentWeapon != null && currentWeapon is MeleeWeapon weapon)
        {
            if (Time.time < lastFireTime + weapon.attackDuration) return;
            playerAnimations.SetAnimation("Attack");
            lastFireTime = Time.time;

        }
        currentWeapon?.Use();
        

    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
      
        if (currentWeapon == rangedWeapon && meleeWeapon != null)
            SwitchWeapon(meleeWeapon);
        else if (currentWeapon == meleeWeapon && rangedWeapon != null)
            SwitchWeapon(rangedWeapon);
        else if (currentWeapon == null && rangedWeapon != null)
            SwitchWeapon(rangedWeapon);
        

    }

    private void OnUnequip(InputAction.CallbackContext context)
    {
        UnequipWeapon();
    }
    private void OnDrop(InputAction.CallbackContext context)
    {
        DropCurrentWeapon();
    }
    private void OnPickup(InputAction.CallbackContext context)
    {
        TryPickupNearbyWeapon();
    }

    private void TryPickupNearbyWeapon()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, pickupLayer);
        foreach (var hit in hits)
        {
            PickupableWeapon pickup = hit.GetComponent<PickupableWeapon>();
            if (pickup != null && !pickup.IsEquipped)
            {
                PickupWeapon(pickup);
                return;
            }
        }
    }

    public void PickupWeapon(PickupableWeapon pickupable)
    {
        Weapon weapon = pickupable.GetWeapon();
        if (weapon == null) return;

        // Drop existing weapon of same type
        Weapon sameType = weapon.weaponType == WeaponType.Ranged ? rangedWeapon : meleeWeapon;
        sameType?.GetComponent<PickupableWeapon>()?.Drop(transform.forward);
        pickupable.Pickup(weaponHolder);
        RegisterWeapon(weapon);

    }

    public void DropCurrentWeapon()
    {
        if (currentWeapon == null) return;

        PickupableWeapon pickup = currentWeapon.GetComponent<PickupableWeapon>();
        pickup?.Drop(transform.forward);

        if (currentWeapon == rangedWeapon)
        {
            rangedWeapon = null;
            rangedIconUI.sprite = null;
            rangedIconUI.enabled = false;
        }
        else if (currentWeapon == meleeWeapon)
        {
            meleeWeapon = null;
            meleeIconUI.sprite = null;
            meleeIconUI.enabled = false;
        }

        currentWeapon = null;
    }


    public void RegisterWeapon(Weapon newWeapon)
    {
        if (newWeapon is RangedWeapon rw)
        {
            rangedWeapon = rw;
            rangedIconUI.sprite = rw.weaponIcon;
            rangedIconUI.enabled = true;
            defultCrosshairUI.SetActive(true);

        }
        else if (newWeapon is MeleeWeapon mw)
        {
            meleeWeapon = mw;
            meleeIconUI.sprite = mw.weaponIcon;
            meleeIconUI.enabled = true;
        }

        SwitchWeapon(newWeapon);
    }


    public void SwitchWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null && currentWeapon.weaponType != newWeapon.weaponType)
        {
            currentWeapon.Unequip();
        }

        currentWeapon = newWeapon;

        if (currentWeapon != null)
        {
            currentWeapon.Equip();
        }
    }

    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
            currentWeapon = null;
        }
    }

    public void UpdateAmmo(int amount)
    {

        totalAmmo += amount;

    }

    public void AmmoUI()
    {

        if (currentWeapon != null)
        {
            if (currentWeapon.weaponType == WeaponType.Ranged)
            {
                ammoText.enabled = true;
                RangedWeapon rw = (RangedWeapon)currentWeapon;
                ammoText.text = $"Current Ammo: {rw.ammoInMagazine} / {rw.totalAmmo}";
            }
            else 
            {
                ammoText.enabled = false;
            }
        }
 
    }


    private void SetAim(bool isAiming)
    {
        if (currentWeapon && currentWeapon == rangedWeapon)
        {
            if (isAiming)
            {
                aimCamera.Priority = 20;
                followCamera.Priority = 10;
                defultCrosshairUI.SetActive(false);
                aimCrosshairUI.SetActive(true);
                playerAnimations.PistolAim(true);

            }
            else
            {
                aimCamera.Priority = 5;
                followCamera.Priority = 10;
                defultCrosshairUI.SetActive(true);
                aimCrosshairUI.SetActive(false);
                playerAnimations.PistolAim(false);
            }
        }
    }

 


}
