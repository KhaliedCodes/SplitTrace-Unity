using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider), typeof(Weapon))]
public class PickupableWeapon : MonoBehaviour
{
    private Rigidbody rb;
    private BoxCollider boxCollider;
    private Weapon weapon;

    public bool IsEquipped { get; private set; } = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        weapon = GetComponent<Weapon>();
        SetPhysicsState(false);
    }


    public void Pickup(Transform holder)
    {
        IsEquipped = true;
        SetPhysicsState(true);

        transform.SetParent(holder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(true);
    }

    public void Drop(Vector3 dropDirection)
    {
        IsEquipped = false;
        SetPhysicsState(false);
        transform.SetParent(null);
        rb.AddForce(dropDirection * 2f, ForceMode.Impulse);
        gameObject.SetActive(true);
    }

    private void SetPhysicsState(bool equipped)
    {
        rb.isKinematic = equipped;
        boxCollider.isTrigger = equipped;
    }

    public Weapon GetWeapon() => weapon;
}
