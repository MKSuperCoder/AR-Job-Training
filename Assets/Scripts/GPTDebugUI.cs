using UnityEngine;
using UnityEngine.UI;

public class GPTDebugUI : MonoBehaviour
{
    public Text debugText;
    private static GPTDebugUI instance;

    private void Awake()
    {
        instance = this;
        if (debugText != null)
            debugText.text = "";
    }

    public static void Show(string prompt, string response)
    {
        if (instance != null && instance.debugText != null)
        {
            instance.debugText.text = $"<b>Prompt:</b> {prompt}\n\n<b>Response:</b> {response}";
        }
    }
}
