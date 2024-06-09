using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LogDisplay : MonoBehaviour
{
    public TextMeshProUGUI logText; // Reference to the TextMesh Pro object
    private string logMessages = ""; // String to hold all log messages

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        logMessages += logString + "\n";
        logText.text = logMessages;
    }
}

