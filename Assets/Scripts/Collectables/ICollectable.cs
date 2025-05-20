using UnityEngine;
public enum Category { 
    LOGS,AMMO,Weapon,Health

}

public interface ICollectable 
{
    int Id { get; set;}
    string Name { get; set; }

    Category Category { get; set; }

    void UpdateState(Category category);
}

