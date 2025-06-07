using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] float speed = 40f;
    [SerializeField] float lifetime = 2f;
    [SerializeField] Transform vfxBlood;
    [SerializeField] Transform vfxHole;
    //[SerializeField] float damage = 10f;
    RangedWeapon rangedWeapon;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }



    private void OnTriggerEnter(Collider other)
    {
        IDamagable target = other.GetComponent<IDamagable>();

        //if (target != null)
        //{

            if (other.CompareTag("Enemy"))
            {
                //target.TakeDamage(rangedWeapon.damage);
               
                    Debug.Log("Hit Enemy: " + other.name + " with damage: " );
                    Transform bloodVFX = Instantiate(vfxBlood, transform.position, Quaternion.identity);
                    Destroy(bloodVFX.gameObject, 2f);
                
            }
        else
        {
            //Transform holeVFX = Instantiate(vfxHole, transform.position, Quaternion.identity);
            //Destroy(holeVFX.gameObject, 2f);
        }
        //}
        //Destroy(gameObject);
    }
}
