using UnityEngine;

public class HealthItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    bool isNear;
    public int      Id { get { return id; } }
    public string   Name { get;  }
    public Category _Category { get { return category; } }

    public bool IsNear { get { return isNear; }set { isNear = value; } }

    private CustomStarterAssetsInputs _input;
    private PlayerHealth _PlayerHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        id = 3;
        category = Category.Health;

    }

    // Update is called once per frame
    void Update()
    {

        if (isNear && _input != null && _input.collect)
        {
            if (_PlayerHealth != null)
            {
                _PlayerHealth.UpdateNumberOfHealthItem(1);
                UiManager.Instance.HidePanelCollecting();
                gameObject.SetActive(false);
                _input.collect = false; // Reset collect input
                isNear = false; // Set isNear to false when player collects the item
            }
        }

    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag =="Player"){
            UiManager.Instance.ShowPanelCollecting();
            _input = other.gameObject.GetComponent<CustomStarterAssetsInputs>();
            _PlayerHealth = other.GetComponent<PlayerHealth>();
            isNear = true; // Set isNear to true when player enters the trigger
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
        _PlayerHealth = null; // Reset PlayerHealth reference when player exits the trigger
    }
}
