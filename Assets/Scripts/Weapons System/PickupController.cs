using System;
using Unity.VisualScripting;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [SerializeField] Transform player, gunHandler;
    [SerializeField] float pickupRange = 2f;
     bool equipped;
    Rigidbody rb;
    BoxCollider boxCollider;
    Weapon weapon;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        weapon = GetComponent<Weapon>();
        if (!equipped)
        {
            rb.isKinematic = false;
            boxCollider.isTrigger = false;
        }
        if(equipped)
        {
            rb.isKinematic = true;
            boxCollider.isTrigger = true;
        }
    }
    private void Update()
    {
        if (!equipped && Input.GetKeyDown(KeyCode.E))
        {
            Pickup();
        }
        if (equipped && Input.GetKeyDown(KeyCode.Q))
        {
            Drop();
        }
    }

    void Pickup()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= pickupRange)
        {

            var manager = player.GetComponent<PlayerManager>();
            if (manager != null && weapon != null)
            {
                Weapon sameType = weapon.weaponType == WeaponType.Ranged ? manager.GetRangedWeapon() : manager.GetMeleeWeapon();
                if (sameType != null)
                sameType.GetComponent<PickupController>()?.Drop();
                equipped = true;
                rb.isKinematic = true;
                boxCollider.isTrigger = true;
                transform.SetParent(gunHandler);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(Vector3.zero);

                manager.RegisterWeapon(weapon);

            }
        }
    }

    private void Drop()
    {
        equipped = false;
        rb.isKinematic = false;
        boxCollider.isTrigger = false;
        transform.SetParent(null);
        gameObject.SetActive(true);
        rb.AddForce(player.forward * 2f, ForceMode.Impulse);

    }

}

