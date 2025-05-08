using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;


public class CoffeeMachineTrainingManager : MonoBehaviour
{
    public TMP_Text customerRequestText; // Assign in Inspector
    public TMP_Text instructionsText;
    public TMP_Text timerText;            // Assign in Inspector
    private ChatGPT chatGPT;

    public List<CustomerRequest> customerRequests = new List<CustomerRequest>();

    private float timeRemaining;
    private bool isTimerRunning = false;
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;
    private int taskCounter = 0;
    private int maxTasks = 3;
    public GameObject finalReportPanel;
    public TMP_Text finalReportText;

    // To store each task's performance
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();


    void Awake()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        chatGPT = FindObjectOfType<ChatGPT>();
    }
    void SaveTaskResult(bool success, float timeLeft)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("User not logged in.");
            return;
        }

        var taskData = new Dictionary<string, object>
    {
        { "request", customerRequestText.text },
        { "completed", success },
        { "timeRemaining", timeLeft },
        { "timeTaken", (success ? (customerRequests.Find(r => r.requestText == customerRequestText.text).timeLimit - timeLeft) : null) },
        { "timestamp", Timestamp.GetCurrentTimestamp() }
    };

        // Save to local list
        taskResults.Add(taskData);

        // Save to Firestore
        firestore.Collection("users")
                 .Document(user.UserId)
                 .Collection("taskResults")
                 .AddAsync(taskData)
                 .ContinueWithOnMainThread(task =>
                 {
                     if (task.IsCompletedSuccessfully)
                     {
                         Debug.Log("Task result saved!");
                     }
                     else
                     {
                         Debug.LogError("Failed to save task result: " + task.Exception);
                     }
                 });

        // Increment task counter
        taskCounter++;

        if (taskCounter > maxTasks)
        {
            EndSession();
        }
        else
        {
            PickRandomRequest();
        }
    }

    void Start()
    {
        SetupCustomerRequests();
        PickRandomRequest();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                timeRemaining = 0;
                isTimerRunning = false;
                TimerFinished();
            }
        }
    }

    void SetupCustomerRequests()
    {
        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make a strong, fresh coffee",
            timeLimit = 60,
            instructions = new string[]
            {
            "Click on the strength button twice.",
            "Click on fresh.",
            "Click on brew."
            }
        });

        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make a mild, fresh coffee",
            timeLimit = 30,
            instructions = new string[]
            {
            "Click on the strength button once.",
            "Click on fresh.",
            "Click on brew."
            }
        });

        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make a regular brew",
            timeLimit = 10,
            instructions = new string[]
            {
            "Click on brew."
            }
        });

        // Add more requests + steps if you want!
    }

    void PickRandomRequest()
    {
        int randomIndex = Random.Range(0, customerRequests.Count);
        CustomerRequest selectedRequest = customerRequests[randomIndex];

        customerRequestText.text = selectedRequest.requestText;
        timeRemaining = selectedRequest.timeLimit;
        isTimerRunning = true;

        // Display instructions
        instructionsText.text = "";
        foreach (string step in selectedRequest.instructions)
        {
            instructionsText.text += "- " + step + "\n";
        }
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds.ToString();
    }

    void TimerFinished()
    {
        Debug.Log("Time is up!");

        // Save the task result (failure)
        SaveTaskResult(false, timeRemaining);

        // Pick the next random request automatically
        PickRandomRequest();
    }
    void EndSession()
    {
        Debug.Log("Training session complete!");

        string performanceSummary = SummarizePerformance(taskResults);

        // Get ChatGPT recommendation
        StartCoroutine(chatGPT.SendPerformanceSummary(performanceSummary, ShowFinalReport));
    }
    string SummarizePerformance(List<Dictionary<string, object>> results)
    {
        int tasksCompleted = 0;
        int tasksFailed = 0;
        float totalTimeTaken = 0f;

        foreach (var task in results)
        {
            if ((bool)task["completed"])
            {
                tasksCompleted++;
                totalTimeTaken += (task["timeTaken"] != null) ? (float)task["timeTaken"] : 0f;
            }
            else
            {
                tasksFailed++;
            }
        }

        float avgTime = tasksCompleted > 0 ? totalTimeTaken / tasksCompleted : 0f;

        return $"The user completed {tasksCompleted} out of {results.Count} tasks successfully. " +
               $"Average time per successful task: {avgTime:F1} seconds. " +
               $"{tasksFailed} tasks were failed.";
    }

    void ShowFinalReport(string aiRecommendation)
    {
        finalReportPanel.SetActive(true);
        finalReportText.text = "Session Complete!\n\n" + aiRecommendation;
    }
    public void OnTaskCompleted()
    {
        SaveTaskResult(true, timeRemaining);
    }

}
