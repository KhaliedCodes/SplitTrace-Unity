using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class LogsSystem : MonoBehaviour
{
    int numberOfLogs=0 ;
    List<LogItem> logs = new List<LogItem>();
    LogItem[] readLogs; 
    public List<LogItem> Logs { get { return logs; } }

   

    void ReadLog(LogsType type) {

        readLogs=logs.ToArray();
        if (readLogs.Length > 0) {
            for (int i = 0; i < readLogs.Length; i++)
            {
                if (readLogs[i].LogType == type) {
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
