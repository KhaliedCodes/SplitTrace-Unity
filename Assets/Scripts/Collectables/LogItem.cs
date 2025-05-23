
using UnityEngine;

public class LogItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    public int Id { get { return id; } }
    public string Name { get;  }
    public Category _Category { get { return category; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        id = 0;
        category = Category.LOGS;
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
        if (other.tag == "Player")
        {
            // update number of logItem with player

            //and give player type of this logs

            //disapper log item

            //play Sound Collect
        }
    }
}
