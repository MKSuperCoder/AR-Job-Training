using UnityEngine;

public class AIResponseTester : MonoBehaviour
{
    void Start()
    {
        // Simulate test prompts (you can replace or expand this list)
        string[] testPrompts = new string[]
        {
            "What is the best way to make coffee?",
            "Explain how a coffee machine works.",
            "Give me safety tips for using this machine."
        };

        foreach (string prompt in testPrompts)
        {
            SimulatePromptResponse(prompt);
        }
    }

    void SimulatePromptResponse(string prompt)
    {
        string fakeResponse = $"This is a simulated response to: \"{prompt}\"";

        // Log both prompt and fake response
        GPTLogger.Instance.LogPrompt(prompt);
        GPTLogger.Instance.LogResponse(fakeResponse);
    }
}
