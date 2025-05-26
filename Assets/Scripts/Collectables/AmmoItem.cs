using UnityEngine;


public class AmmoItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;

    int itemAmount =15;
    public int Id { get { return id; } }
    public string Name { get;  }
    public Category _Category { get { return category; } }

    void Start()
    {
        id = 1;
        category = Category.AMMO;
    }


    void Update()
    {

    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") {

            other.GetComponent<WeaponManager>().GetUdateAmmo(itemAmount, 1);

            gameObject.SetActive(false);

            //play Sound Collect
        }
    }
}
