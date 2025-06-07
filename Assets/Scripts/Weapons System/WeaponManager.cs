using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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
    [SerializeField] CinemachineVirtualCamera aimCamera;

    [Header("Aiming UI")]
    [SerializeField] GameObject defultCrosshairUI;
    [SerializeField] GameObject aimCrosshairUI;
    [SerializeField] Transform aimTarget;


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


    public Vector3 mouseWorldPosition { get; private set; } = Vector3.zero;

    public bool IsAiming { get; private set; }

    public static WeaponManager Instance;

    //TUT Script
    [SerializeField] TUT WeaponTut;

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
        
        aimLayerCurrentWeight = Mathf.Lerp(aimLayerCurrentWeight, aimLayerTargetWeight, Time.deltaTime * aimLayerSmoothSpeed);
        animator.SetLayerWeight(1, aimLayerCurrentWeight);

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWight, Time.deltaTime * 20);
        //aimLayerTargetWeight = currentWeapon &&currentWeapon == rangedWeapon ? 1f : 0f;
        UpdateAnimatorRangedType();

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, aimLayerMask))
        {
            aimTarget.position = hit.point;

            mouseWorldPosition = hit.point;
        }
        else
        {
            mouseWorldPosition = ray.origin + ray.direction * 100f;
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
        AmmoUI();
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
        currentWeapon?.Use(mouseWorldPosition);
        AmmoUI();

    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {

        if (currentWeapon == rangedWeapon && meleeWeapon != null && IsAiming == false)
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
        if(!IsAiming)
        DropCurrentWeapon();
        AmmoUI();
    }
    private void OnPickup(InputAction.CallbackContext context)
    {
        TryPickupNearbyWeapon();

        WeaponTut.OnTutorialStart("Weapon");

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
        AmmoUI();
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
        AmmoUI();
    }

    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
            currentWeapon = null;
        }
    }

    private bool HasRangedEquipped(out RangedWeapon rw)
    {
        rw = currentWeapon as RangedWeapon;
        return rw != null;
    }
    public void UpdateAmmo(int amount)
    {
        managerTotalAmmo += amount;
        Debug.Log($"Collected ammo: {amount}, Stored: {managerTotalAmmo}");
        if (HasRangedEquipped(out RangedWeapon rw))
        {
             rw = (RangedWeapon)currentWeapon;
            rw.UpdateAmmoNumber(managerTotalAmmo);
            managerTotalAmmo = 0;
            AmmoUI();
        }

    }


    public void AmmoUI()
    {
        if (currentWeapon is RangedWeapon rw)
        {
            ammoText.enabled = true;
            ammoText.text = $"Current Ammo: {rw.ammoInMagazine} / {rw.totalAmmo + managerTotalAmmo}";
            defultCrosshairUI.SetActive(!IsAiming);
            Debug.Log("in ranged");
        }
        else
        {
            Debug.Log("in melee");
            ammoText.enabled = false;
            defultCrosshairUI.SetActive(false);
        }
    }



    private void SetAim(bool isAiming)
    {
        if (HasRangedEquipped(out RangedWeapon rw))
        {
            IsAiming = isAiming;
            
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

        if (HasRangedEquipped(out RangedWeapon rw))
        {
            if (rangedWeapon.rangedType == 1)
                rangedTypeValue = 1;
            else if (rangedWeapon.rangedType == 2)
                rangedTypeValue = 2;
        }

        animator.SetInteger("RangedType", rangedTypeValue);
    }


    
}