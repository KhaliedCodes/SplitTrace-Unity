using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] Transform weaponHolder;
    Weapon currentWeapon;
    RangedWeapon rangedWeapon;
    MeleeWeapon meleeWeapon;


    void Update()
    {
        if (currentWeapon != null && currentWeapon.transform.IsChildOf(weaponHolder))
        {

            if (Input.GetMouseButtonDown(0))
            {
                currentWeapon.Use();
            }


            if (Input.GetKeyDown(KeyCode.R) && currentWeapon is RangedWeapon ranged)
            {
                currentWeapon.Reload();
            }



        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchWeapon(rangedWeapon);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchWeapon(meleeWeapon);
        }

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
