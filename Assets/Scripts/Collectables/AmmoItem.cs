using UnityEngine;
using UnityEngine.Windows;


public class AmmoItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    bool isNear;
    int itemAmount =15;
    public int Id { get { return id; } }
    public string Name { get;  }
    public Category _Category { get { return category; } }

    public bool IsNear { get { return isNear; } set { isNear = value; } }

    private CustomStarterAssetsInputs _input;
    private WeaponManager _weaponManager;
    private void Awake()
    {
       
    }
    void Start()
    {
        id = 1;
        category = Category.AMMO;
    }


    void Update()
    {
        if (isNear && _input!=null && _input.collect)
        {
            if(_weaponManager != null)
            {
                _weaponManager.UpdateAmmo(itemAmount);
                UiManager.Instance.HidePanelCollecting();
                gameObject.SetActive(false);
                _input.collect = false; // Reset collect input
                isNear = false;
            }
            else
            {
                Debug.LogError("WeaponManager is not assigned or found on the player.");
            }
        }
    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") {
            _input = other.gameObject.GetComponent<CustomStarterAssetsInputs>();
            UiManager.Instance.ShowPanelCollecting();
            isNear = true; // Set isNear to true when player enters the trigger
            _weaponManager= other.GetComponent<WeaponManager>();
        }
        else
        {
            UiManager.Instance.HidePanelCollecting();

        }
    }
    private void OnTriggerExit(Collider other)
    {
        UiManager.Instance.HidePanelCollecting();
        isNear = false; // Set isNear to true when player enters the trigger
        _input = null; // Reset input reference when player exits the trigger
    }
}
