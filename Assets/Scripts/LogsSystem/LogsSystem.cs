using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LogsSystem : MonoBehaviour
{
 
    List<LogItem> logs = new List<LogItem>();
  
    public List<LogItem> Logs { get { return logs; } }

   

    void ReadLog(LogsType type) {

        if (logs.ToArray().Length > 0) {
            foreach (LogItem item in logs) {
                if (item.LogType == type)
                {
                    // write this log in Text Box
                    // Active Text Box
                }
            }
           
        
        }
    }

    void CloseLog() {
        //dis  Active Text Box

    }

}
