using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class LogItem : MonoBehaviour, ICollectable
{
    int id;
    Category category;
    public int Id { get { return id; } set { id = value; } }
    public string Name { get; set; }
    public Category Category { get { return category; } set { category = value; } }

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
