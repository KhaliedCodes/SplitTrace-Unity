using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class HealthItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    public int      Id { get { return id; } }
    public string   Name { get;  }
    public Category _Category { get { return category; } }





    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        id = 3;
        category = Category.Health;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag =="Player"){
            CallingUIHint();
            if (Input.GetKey(KeyCode.E)) {

                other.gameObject.GetComponent<PlayerHealth>().UpdateNumberOfHealthItem();
                gameObject.SetActive(false);
                HideUIHint();
            }

            //play Sound Collect

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
