using UnityEngine;


public class Ammo : MonoBehaviour ,ICollectable
{
    int id;
    string name;
    Category category;
    public int       Id { get { return id; } set { id =value; } }
    public string    Name { get { return name; } set {name=value; } }
    public Category  _Category { get { return category; } set { category = value; } }

    void Start()
    {
        
    }

   
    void Update()
    {
        
    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
}
