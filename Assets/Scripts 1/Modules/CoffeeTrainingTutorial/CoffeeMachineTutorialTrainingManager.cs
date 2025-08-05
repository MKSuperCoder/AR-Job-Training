using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoffeeMachineTutorialTrainingManager : MonoBehaviour
{
    public TMP_Text customerRequestText;
    public TMP_Text instructionsText;
    public TMP_Text timerText;
    public TMP_Text errorMessageText;
    public GameObject finalReportPanel;
    public TMP_Text finalReportText;
    public GameObject errorOverlayPanel;
    public UnityEngine.UI.Button continueButton;
    public GameObject tutorialPopupPanel;
    public TMP_Text tutorialPopupText;
    public UnityEngine.UI.Button tutorialContinueButton;

    private ChatGPT chatGPT;
    private UserSession session;

    public List<CustomerRequest> customerRequests = new List<CustomerRequest>();
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();
    public List<ARClickHandler.ButtonType> userActions = new List<ARClickHandler.ButtonType>();

    private float timeRemaining;
    private bool isTimerRunning = false;
    private int taskCounter = 0;
    private int maxTasks = 3;

    public GameObject[] tutorial3Instructions;
    private int currentInstructionIndex = 0;

    void Awake()
    {
        session = UserSession.Instance;
        session.LoadFromPrefs();

        chatGPT = FindObjectOfType<ChatGPT>();
        if (chatGPT == null)
            Debug.LogWarning("ChatGPT object not found. AI feedback will not be available.");
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
        var request = customerRequestText.text;
        var matchedRequest = customerRequests.Find(r => r.requestText == request);
        if (matchedRequest == null) return;

        var timeTaken = success ? matchedRequest.timeLimit - timeLeft : (float?)null;

        var result = new Dictionary<string, object>
        {
            { "request", request },
            { "completed", success },
            { "timeRemaining", timeLeft },
            { "timeTaken", timeTaken },
        };

        taskResults.Add(result);
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
        return $"Completed: {completed}/{results.Count}\n⏱ Avg Time: {avgTime:F1}s\nFailed: {failed}";
    }

    void ShowFinalReport(string aiFeedback)
    {
        finalReportPanel.SetActive(true);
        finalReportText.gameObject.SetActive(true);
        finalReportText.text = "Session Complete!\n\n" + aiFeedback;
    }

    public void ShowErrorMessage(string message)
    {
        isTimerRunning = false;
        errorOverlayPanel.SetActive(true);
        errorMessageText.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
        errorMessageText.text = message;

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(HideErrorOverlay);
    }

    public void HideErrorOverlay()
    {
        errorOverlayPanel.SetActive(false);
        isTimerRunning = true;
    }

    public void OnContinueButtonClicked()
    {
        continueButton?.gameObject.SetActive(false);
        errorMessageText?.gameObject.SetActive(false);
        errorOverlayPanel?.SetActive(false);
        isTimerRunning = true;
    }

    public void StartTrainingAfterPopup()
    {
        tutorialPopupPanel?.SetActive(false);
        PickRandomRequest();
    }


    public void ShowNextInstruction()
    {
        if (currentInstructionIndex < tutorial3Instructions.Length)
        {
            tutorial3Instructions[currentInstructionIndex].SetActive(false);
            currentInstructionIndex++;

            if (currentInstructionIndex < tutorial3Instructions.Length)
                tutorial3Instructions[currentInstructionIndex].SetActive(true);
        }
    }

    public void DisableTutorialPopup()
    {
        tutorialPopupPanel?.SetActive(false);
    }
}
