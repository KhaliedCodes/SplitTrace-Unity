using UnityEngine;

public enum WeaponType { Melee, Ranged }

public abstract class Weapon : MonoBehaviour
{
    public string weaponName;
    public bool isEquipped;
    public WeaponType weaponType;
    public Sprite weaponIcon;

    public virtual void Equip()
    {
        isEquipped = true;
        gameObject.SetActive(true);
    }
    public virtual void Unequip()
    {
        isEquipped = false;
        gameObject.SetActive(false);
    }

    public abstract void Use();
    public abstract void Reload();

}
