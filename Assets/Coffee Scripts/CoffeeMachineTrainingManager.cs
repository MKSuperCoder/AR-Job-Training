using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;

public class CoffeeMachineTrainingManager : MonoBehaviour
{
    public TMP_Text customerRequestText;
    public TMP_Text instructionsText;
    public TMP_Text timerText;
    public GameObject finalReportPanel;
    public TMP_Text finalReportText;

    private ChatGPT chatGPT;
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;

    private float timeRemaining;
    private bool isTimerRunning = false;

    public List<CustomerRequest> customerRequests = new List<CustomerRequest>();
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();
    private int taskCounter = 0;
    private int maxTasks => customerRequests.Count;

    void Awake()
    {
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        chatGPT = FindObjectOfType<ChatGPT>();
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
    }

    void PickRandomRequest()
    {
        if (taskCounter >= maxTasks)
        {
            EndSession();
            return;
        }

        int randomIndex = Random.Range(0, customerRequests.Count);
        CustomerRequest selectedRequest = customerRequests[randomIndex];

        customerRequestText.text = selectedRequest.requestText;
        timeRemaining = selectedRequest.timeLimit;
        isTimerRunning = true;

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
        SaveTaskResult(false, timeRemaining);
        PickRandomRequest();
    }

    public void OnTaskCompleted()
    {
        SaveTaskResult(true, timeRemaining);
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
            { "timeTaken", success ? (customerRequests.Find(r => r.requestText == customerRequestText.text).timeLimit - timeLeft) : 0 },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        taskResults.Add(taskData);

        firestore.Collection("users")
            .Document(user.UserId)
            .Collection("taskResults")
            .AddAsync(taskData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                    Debug.Log("Task result saved!");
                else
                    Debug.LogError("Failed to save task result: " + task.Exception);
            });

        taskCounter++;

        if (taskCounter >= maxTasks)
            EndSession();
        else
            PickRandomRequest();
    }

    void EndSession()
    {
        Debug.Log("Training session complete!");
        string performanceSummary = SummarizePerformance(taskResults);
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
                totalTimeTaken += (float)task["timeTaken"];
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
}
