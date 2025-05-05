using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using System;

public class ChatGPT : MonoBehaviour
{
    private string apiKey = "";
    private string apiURL = "https://api.openai.com/v1/chat/completions";

    void Start()
    {
        // Load API key from .env
        string envPath = Path.Combine(Application.streamingAssetsPath, ".env");
        var env = EnvLoader.LoadEnv(envPath);

        if (env.ContainsKey("OPENAI_API_KEY"))
        {
            apiKey = env["OPENAI_API_KEY"];
            Debug.Log("API key loaded.");
        }
        else
        {
            Debug.LogWarning("API key not found in .env!");
        }
    }

    // Call this to send performance summary to ChatGPT
    public IEnumerator SendPerformanceSummary(string performanceSummary, Action<string> onResponse)
    {
        // Set up the chat request
        ChatGPTRequest requestData = new ChatGPTRequest
        {
            model = "gpt-3.5-turbo",
            messages = new Message[]
            {
                new Message { role = "system", content = "You are a friendly training coach. Give helpful feedback based on the user's performance." },
                new Message { role = "user", content = "Here is the user's training performance: " + performanceSummary }
            }
        };

        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log("Sending JSON to ChatGPT: " + jsonData);

        var request = new UnityWebRequest(apiURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ChatGPTResponse response = JsonUtility.FromJson<ChatGPTResponse>(request.downloadHandler.text);
            if (response != null && response.choices.Length > 0)
            {
                Debug.Log("ChatGPT Recommendation Received.");
                onResponse(response.choices[0].message.content);
            }
            else
            {
                Debug.LogWarning("ChatGPT gave an empty response.");
                onResponse("No feedback available.");
            }
        }
        else
        {
            Debug.LogError("ChatGPT request failed: " + request.error);
            onResponse("Error getting feedback.");
        }
    }
}
