using UnityEngine;
using UnityEngine.InputSystem;


public class AmmoItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;

    int itemAmount =15;
    public int Id { get { return id; } }
    public string Name { get;  }
    public Category _Category { get { return category; } }
    bool collectingProcess, thereExistItem;

    void Start()
    {
        id = 1;
        category = Category.AMMO;

    }


    void Update()
    {
        if (Input.GetKey(KeyCode.E)&& thereExistItem) { 
             collectingProcess=true;
            thereExistItem=false;
        }
    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") {
            thereExistItem=true;
            CallingUIHint();
            if (collectingProcess) { 
                other.GetComponent<WeaponManager>().UpdateAmmo(itemAmount);
                //play Sound Collect
                gameObject.SetActive(false);
                HideUIHint();
                collectingProcess=false;
            }

            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        HideUIHint();

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
