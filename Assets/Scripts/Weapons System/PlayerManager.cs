using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Transform weaponHolder;
    Weapon currentWeapon;
    RangedWeapon rangedWeapon;
    MeleeWeapon meleeWeapon;

    WeaponsInputSystem weaponInputs;


    private void Awake()
    {
        weaponInputs = new WeaponsInputSystem();
        weaponInputs.Enable();
        weaponInputs.WeaponsActions.Reload.performed += OnReload;
        weaponInputs.WeaponsActions.Shoot.performed += OnShoot;
        weaponInputs.WeaponsActions.SwitchWeapon.performed += OnSwitchWeapon;

    }

    private void OnSwitchWeapon(InputAction.CallbackContext context)
    {
        if (currentWeapon != null && currentWeapon.transform.IsChildOf(weaponHolder))
        {
            SwitchWeapon(currentWeapon == rangedWeapon ? meleeWeapon : rangedWeapon);
        }
        
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        if (currentWeapon != null && currentWeapon.transform.IsChildOf(weaponHolder))
        {
            currentWeapon.Reload();
        }
    }
    private void OnShoot(InputAction.CallbackContext context)
    {
        if (currentWeapon != null && currentWeapon.transform.IsChildOf(weaponHolder))
        {   
            currentWeapon.Use();
        }
    }

    public void RegisterWeapon(Weapon newWeapon)
    {
        if (newWeapon is RangedWeapon rw)
            rangedWeapon = rw;
        else if (newWeapon is MeleeWeapon mw)
            meleeWeapon = mw;

        dropWeapon(newWeapon);
    }


    public void SwitchWeapon(Weapon newWeapon)
    {
           if (currentWeapon != null) 
    {
        currentWeapon.Unequip();
    }

        currentWeapon = newWeapon;
        if (currentWeapon != null)
        {
            currentWeapon.Equip();
            Debug.Log("Switched to: " + currentWeapon.weaponName);
        }
    
    }

    public void dropWeapon(Weapon newWeapon)
    {
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
            currentWeapon.gameObject.SetActive(true);
        }

        currentWeapon = newWeapon;
        if (currentWeapon != null)
        {
            currentWeapon.Equip();
        }

    }


    public Weapon GetRangedWeapon() => rangedWeapon;
    public Weapon GetMeleeWeapon() => meleeWeapon;
}
