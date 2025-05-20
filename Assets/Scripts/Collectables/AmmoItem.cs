using UnityEngine;


public class AmmoItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    public int Id { get { return id; } set { id = value; } }
    public string Name { get; set; }
    public Category Category { get { return category; } set { category = value; } }

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
