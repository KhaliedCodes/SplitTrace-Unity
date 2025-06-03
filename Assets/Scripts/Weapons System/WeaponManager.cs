using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.UI;
//using UnityEngine.UIElements;


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
    [SerializeField] CinemachineVirtualCamera aimCamera;

    [Header("Aiming UI")]
    [SerializeField] GameObject defultCrosshairUI;
    [SerializeField] GameObject aimCrosshairUI;
    [SerializeField] Transform aimTarget;

    
    //[SerializeField] Transform aimTarget;
    //[SerializeField] Transform projectileSpawnPoint;
    [Header("Animation Rig")]
    [SerializeField] Rig aimRig;
    [SerializeField] LayerMask aimLayerMask;

    private float aimLayerTargetWeight = 0f;
    private float aimLayerCurrentWeight = 0f;
    private float aimLayerSmoothSpeed = 10f;
    private float lastFireTime = -999f;
    private float aimRigWight = 1f;
    private int managerTotalAmmo;

    private CustomThridPersonController playerController;
    private Weapon currentWeapon;
    private RangedWeapon rangedWeapon;
    private MeleeWeapon meleeWeapon;
    private WeaponsInputSystem weaponInputs;
    private PlayerAnimations playerAnimations;

    public Animator animator;   
    bool isAimingMode = false;
    
    public bool IsAiming { get; private set; }

    public static WeaponManager Instance;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        playerController = GetComponent<CustomThridPersonController>();
        weaponInputs = new WeaponsInputSystem();
        weaponInputs.Enable();
        weaponInputs.WeaponsActions.Reload.performed += OnReload;
        weaponInputs.WeaponsActions.SwitchWeapon.performed += OnSwitchWeapon;
        weaponInputs.WeaponsActions.Unequip.performed += OnUnequip;
        weaponInputs.WeaponsActions.Drop.performed += OnDrop;
        weaponInputs.WeaponsActions.Pickup.performed += OnPickup;
        weaponInputs.WeaponsActions.Shoot.performed += ctx => OnShoot(true);
        weaponInputs.WeaponsActions.Shoot.canceled += ctx => OnShoot(false);
        weaponInputs.WeaponsActions.Aim.performed += ctx => SetAim(true);
        weaponInputs.WeaponsActions.Aim.canceled += ctx => SetAim(false);
    }

    private void Start()
    {
        rangedIconUI.enabled = false;
        meleeIconUI.enabled = false;
        ammoText.enabled = false;
        playerAnimations = GetComponent<PlayerAnimations>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        AmmoUI();
        aimLayerCurrentWeight = Mathf.Lerp(aimLayerCurrentWeight, aimLayerTargetWeight, Time.deltaTime * aimLayerSmoothSpeed);
        animator.SetLayerWeight(1, aimLayerCurrentWeight);

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWight, Time.deltaTime * 20);
        //aimLayerTargetWeight = currentWeapon &&currentWeapon == rangedWeapon ? 1f : 0f;
        UpdateAnimatorRangedType();

        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
        {
            aimTarget.position = hit.point;

            mouseWorldPosition = hit.point;
        }
        if (IsAiming)
        {
            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
            playerController.SetRotateOnMove(false);
            aimRig.weight = 1f; 
        }
        else
        {
            playerController.SetRotateOnMove(true);
            aimRig.weight = 0f; 

        }
    }



    private void OnReload(InputAction.CallbackContext context)
    {
        currentWeapon?.Reload();
    }

    private void OnShoot(bool IsShooting)
    {
        if (!currentWeapon) return;

        if (currentWeapon != null && currentWeapon is MeleeWeapon weapon)
        {
            if (Time.time < lastFireTime + weapon.attackDuration) return;
            playerAnimations.SetAnimation("Attack");
            lastFireTime = Time.time;

        }
        else
        {
            animator.SetBool("Shoot", IsShooting);
        }
        animator.SetTrigger("Aim");
        currentWeapon?.Use();


    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {

        if (currentWeapon == rangedWeapon && meleeWeapon != null && isAimingMode == false)
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

            rw.UpdateAmmoNumber(managerTotalAmmo);
            managerTotalAmmo = 0;


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
        managerTotalAmmo += amount;
        Debug.Log($"Collected ammo: {amount}, Stored: {managerTotalAmmo}");

        if (currentWeapon != null && currentWeapon.weaponType == WeaponType.Ranged)
        {
            RangedWeapon rw = (RangedWeapon)currentWeapon;
            rw.UpdateAmmoNumber(managerTotalAmmo);
            managerTotalAmmo = 0;

        }
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
                defultCrosshairUI.SetActive(true);
            }
            else
            {
                ammoText.enabled = false;
                defultCrosshairUI.SetActive(false);
            }
        }

    }


    private void SetAim(bool isAiming)
    {
        if (currentWeapon && currentWeapon == rangedWeapon)
        {
            IsAiming = isAiming;
            isAimingMode = isAiming ? true : false;
            
            aimCamera.Priority = isAiming ? 20 : 5;
            defultCrosshairUI.SetActive(!isAiming);
            aimCrosshairUI.SetActive(isAiming);

            aimLayerTargetWeight = isAiming ? 1f : 0f;
            animator.SetBool("IsAiming", isAiming);
            animator.SetInteger("RangedType", (int)((RangedWeapon)currentWeapon).rangedType);
         
      
        }
        else
        {
            //defultCrosshairUI.SetActive(false);
            animator.SetBool("IsAiming", false);
            animator.SetInteger("RangedType", 0);
        }

    }

    private void UpdateAnimatorRangedType()
    {
        int rangedTypeValue = 0;

        if (currentWeapon && currentWeapon.weaponType == WeaponType.Ranged)
        {
            if (rangedWeapon.rangedType == 1)
                rangedTypeValue = 1;
            else if (rangedWeapon.rangedType == 2)
                rangedTypeValue = 2;
        }

        animator.SetInteger("RangedType", rangedTypeValue);
    }


    
}