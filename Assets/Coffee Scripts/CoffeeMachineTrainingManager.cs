using System.Collections;
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
    public TMP_Text errorMessageText;
    public GameObject finalReportPanel;
    public TMP_Text finalReportText;
    public GameObject errorOverlayPanel;
    public UnityEngine.UI.Button continueButton;


    private ChatGPT chatGPT;
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;

    public List<CustomerRequest> customerRequests = new List<CustomerRequest>();
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();
    public List<ARClickHandler.ButtonType> userActions = new List<ARClickHandler.ButtonType>();

    private float timeRemaining;
    private bool isTimerRunning = false;
    private int taskCounter = 0;
    private int maxTasks = 3;

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
        if (isTimerRunning && timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerUI();
        }
        else if (isTimerRunning)
        {
            timeRemaining = 0;
            isTimerRunning = false;
            TimerFinished();
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
            },
            expectedActions = new ARClickHandler.ButtonType[]
            {
                ARClickHandler.ButtonType.Strength,
                ARClickHandler.ButtonType.Strength,
                ARClickHandler.ButtonType.Fresh,
                ARClickHandler.ButtonType.Brew
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
            },
            expectedActions = new ARClickHandler.ButtonType[]
            {
                ARClickHandler.ButtonType.Strength,
                ARClickHandler.ButtonType.Fresh,
                ARClickHandler.ButtonType.Brew
            }
        });

        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make a regular brew",
            timeLimit = 10,
            instructions = new string[]
            {
                "Click on brew."
            },
            expectedActions = new ARClickHandler.ButtonType[]
            {
                ARClickHandler.ButtonType.Brew
            }
        });
    }

    void PickRandomRequest()
    {
        userActions.Clear();
        int index = Random.Range(0, customerRequests.Count);
        var request = customerRequests[index];

        customerRequestText.text = request.requestText;
        instructionsText.text = string.Join("\n", request.instructions);
        timeRemaining = request.timeLimit;
        isTimerRunning = true;
    }

    void UpdateTimerUI()
    {
        timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }

    void TimerFinished()
    {
        Debug.Log("Time is up!");
        ShowErrorMessage("You ran out of time!");
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

        var request = customerRequestText.text;
        var timeTaken = success ? customerRequests.Find(r => r.requestText == request).timeLimit - timeLeft : (float?)null;

        var result = new Dictionary<string, object>
        {
            {"request", request},
            {"completed", success},
            {"timeRemaining", timeLeft},
            {"timeTaken", timeTaken},
            {"timestamp", Timestamp.GetCurrentTimestamp() }
        };

        taskResults.Add(result);

        firestore.Collection("users")
                 .Document(user.UserId)
                 .Collection("taskResults")
                 .AddAsync(result)
                 .ContinueWithOnMainThread(task =>
                 {
                     if (!task.IsCompletedSuccessfully)
                         Debug.LogError("Failed to save: " + task.Exception);
                 });

        taskCounter++;

        if (taskCounter >= maxTasks)
            EndSession();
        else
            PickRandomRequest();
    }

    void EndSession()
    {
        string summary = SummarizePerformance(taskResults);
        StartCoroutine(chatGPT.SendPerformanceSummary(summary, ShowFinalReport));
    }

    string SummarizePerformance(List<Dictionary<string, object>> results)
    {
        int completed = 0, failed = 0;
        float totalTime = 0f;

        foreach (var r in results)
        {
            if ((bool)r["completed"])
            {
                completed++;
                if (r["timeTaken"] != null)
                    totalTime += (float)r["timeTaken"];
            }
            else failed++;
        }

        float avgTime = completed > 0 ? totalTime / completed : 0f;

        return $"Completed {completed}/{results.Count}. Avg time: {avgTime:F1}s. Failed: {failed}.";
    }

    void ShowFinalReport(string aiFeedback)
    {
        finalReportPanel.SetActive(true);
        finalReportText.text = "Session Complete!\n\n" + aiFeedback;
    }

    public void ShowErrorMessage(string message)
    {
        isTimerRunning = false; // Pause timer
        errorOverlayPanel.SetActive(true);
        errorMessageText.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        errorMessageText.text = message;

        // Add listener to resume when button is clicked
        continueButton.onClick.RemoveAllListeners(); // Prevent stacking events
        continueButton.onClick.AddListener(HideErrorOverlay);
    }


    private IEnumerator HideErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorMessageText.gameObject.SetActive(false);
    }
    public bool RegisterButtonClick(ARClickHandler.ButtonType clickedButton)
    {
        var currentRequest = customerRequests.Find(r => r.requestText == customerRequestText.text);
        userActions.Add(clickedButton);

        // If user has clicked more than expected steps, it's an error
        if (userActions.Count > currentRequest.expectedActions.Length)
        {
            ShowErrorMessage("Too many steps!");
            return false;
        }

        // Compare each clicked action to expected
        for (int i = 0; i < userActions.Count; i++)
        {
            if (userActions[i] != currentRequest.expectedActions[i])
            {
                ShowErrorMessage("Wrong action!");
                return false;
            }
        }

        return true; // So far, it's valid
    }
    public void HideErrorOverlay()
    {
        errorOverlayPanel.SetActive(false);
        isTimerRunning = true;
    }
    public void OnContinueButtonClicked()
    {
        // Disable UI elements
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (errorMessageText != null) errorMessageText.gameObject.SetActive(false);
        if (errorOverlayPanel != null) errorOverlayPanel.SetActive(false);

        // Resume the timer
        isTimerRunning = true;
    }

}
