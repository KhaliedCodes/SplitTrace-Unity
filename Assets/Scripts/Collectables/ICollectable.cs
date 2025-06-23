using UnityEngine;
public enum Category { 
    LOGS,AMMO,Weapon,Health

}

public interface ICollectable 
{
    int Id { get;}
    string Name { get;  }
   
    bool IsNear { get; set; }
    Category _Category { get; }
   
    
        
    void UpdateState(Category category);
}

