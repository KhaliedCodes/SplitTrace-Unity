using UnityEngine;
using UnityEngine.InputSystem;


public class AmmoItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;

    int itemAmount =15;
    public int Id { get { return id; } }
    public string Name { get;  }
    public int ItemAmount { get { return itemAmount; } }
    public Category _Category { get { return category; } }

    public void UpdateState(Category category)
    {
      
    }

    void Start()
    {
        id = 1;
        category = Category.AMMO;

    }


   
   
}
