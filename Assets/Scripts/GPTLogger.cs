using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GPTLogger : MonoBehaviour
{
    public static GPTLogger Instance;

    public Text debugText; // Optional: assign in the Inspector

    private string promptLogPath;
    private string responseLogPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            promptLogPath = Path.Combine(Application.persistentDataPath, "prompt_log.txt");
            responseLogPath = Path.Combine(Application.persistentDataPath, "response_log.txt");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LogPrompt(string prompt)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        File.AppendAllText(promptLogPath, $"[{timestamp}] {prompt}\n");
        UpdateDebugPanel($"User: {prompt}");
    }

    public void LogResponse(string response)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        File.AppendAllText(responseLogPath, $"[{timestamp}] {response}\n");
        UpdateDebugPanel($"GPT: {response}");
    }

    private void UpdateDebugPanel(string message)
    {
        if (debugText != null)
        {
            debugText.text += message + "\n\n";
        }
    }
}
