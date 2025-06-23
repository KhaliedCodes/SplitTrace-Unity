
using Unity.VisualScripting;
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
    [SerializeField] bool isNear;
    public int Id { get { return id; } }
    public string Name { get;  }
    public Category _Category { get { return category; } }
    public LogsType LogType { get { return logType; } }
    public string LogContent { get { return contentOfLog; } }

    public bool IsNear { get { return isNear; } set { isNear = value; } }

    private CustomStarterAssetsInputs _input;
    private LogsSystem _logsSystem;
    public void UpdateState(Category _category)
    {
        // update here new state of player after collect
    }
    private void Update()
    {
        if (isNear && _input != null && _input.collect)
        {
            if (_logsSystem != null)
            {
                _logsSystem.Logs.Add(this);
                UiManager.Instance.HidePanelCollecting();
                gameObject.SetActive(false);
                _input.collect = false; // Reset collect input
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            UiManager.Instance.ShowPanelCollecting();
            _input = other.gameObject.GetComponent<CustomStarterAssetsInputs>();
            isNear = true; // Set isNear to true when player enters the trigger
            _logsSystem = other.GetComponent<LogsSystem>();

        }
        else { 
            UiManager.Instance.HidePanelCollecting();
        
        }
    }
    private void OnTriggerExit(Collider other)
    {
        UiManager.Instance.HidePanelCollecting();
        isNear = false; // Set isNear to false when player exits the trigger
        _input = null; // Reset input reference when player exits the trigger
    }
}
