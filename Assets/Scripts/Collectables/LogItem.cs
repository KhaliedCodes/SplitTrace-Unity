
using UnityEngine;
public enum LogsType { 
    First,Second,Third
}
public class LogItem : MonoBehaviour, ICollectable
{
    [SerializeField] private int id=0;
    [SerializeField] private Category category = Category.LOGS;
    [SerializeField] private LogsType logType=LogsType.First;
    [SerializeField] private string contentOfLog;
    public int Id { get { return id; } }
    public string Name { get;  }
    public Category _Category { get { return category; } }
    public LogsType LogType { get { return logType; } }
    public string LogContent { get { return contentOfLog; } }
   

    private void Update()
    {
        
    }
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void OnTriggerEnter(Collider other)
    {
        
    }
  
}
