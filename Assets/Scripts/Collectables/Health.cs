using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Health : MonoBehaviour, ICollectable
{
    int id;
    string name;
    Category category;
    public int      Id { get { return id; } set { id =value; } }
    public string   Name { get { return name; } set {name=value; } }
    public Category _Category { get { return category; } set { category = value; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
}
