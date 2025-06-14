using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] float speed = 40f;
    [SerializeField] float lifetime = 2f;
    [SerializeField] Transform vfxBlood;
    [SerializeField] Transform vfxHole;
    [SerializeField] float damage = 10f;
    Rigidbody rb;


    public void init(float damage)
    {
        this.damage = damage;
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }



    private void OnTriggerEnter(Collider other)
    {
        IDamagable target = other.GetComponent<IDamagable>();
        Debug.Log("Hit the enemy");
        if (target != null)
        {

            Debug.Log("the tag is : " + other.tag);
            Debug.Log(other.tag == "Enemy");

            if (other.tag == "Enemy")
            {
                Debug.Log("Hit Enemy: " + other.name + " with damage: ");
                target.TakeDamage(damage);

                    Transform bloodVFX = Instantiate(vfxBlood, transform.position, Quaternion.identity);
                    Destroy(bloodVFX.gameObject, 2f);
                
            }
       
        }
        //Destroy(gameObject);
    }
}
