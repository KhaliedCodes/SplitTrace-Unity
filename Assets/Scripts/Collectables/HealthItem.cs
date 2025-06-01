using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class HealthItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    public int      Id { get { return id; } }
    public string   Name { get;  }
    public Category _Category { get { return category; } }
    bool collectingProcess, thereExistItem;




    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        id = 3;
        category = Category.Health;

    }

    // Update is called once per frame
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
        if (other.tag =="Player"){
            thereExistItem=true;
            CallingUIHint();
            if (collectingProcess) {

                other.gameObject.GetComponent<PlayerHealth>().UpdateNumberOfHealthItem();
                gameObject.SetActive(false);
                HideUIHint();
            }

            //play Sound Collect

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
