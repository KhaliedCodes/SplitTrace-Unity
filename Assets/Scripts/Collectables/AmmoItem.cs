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

            CallingUIHint();
            if (Input.GetKey(KeyCode.E)) { 
                other.GetComponent<WeaponManager>().UpdateAmmo(itemAmount);
                //play Sound Collect
                gameObject.SetActive(false);
                HideUIHint();
            }

            
        }
    }

    public void CallingUIHint()
    {
        UiManager.Instance.DisplayPickUp();
    }

    public void HideUIHint()
    {
        UiManager.Instance.ClosePickUpPanel();
    }
}
