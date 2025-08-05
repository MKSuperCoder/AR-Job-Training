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
    public TMP_Text trainerHintText;
    public TMP_Text coffeeTimeText;
    private Coroutine countdownCoroutine;
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;
    private ChatGPT chatGPT;
    private UserSession session;
    public TMP_Text strengthText;

    public List<ARClickHandler.ButtonType> pressedButtons = new List<ARClickHandler.ButtonType>();
    public List<CoffeeRecipe> coffeeRecipes = new List<CoffeeRecipe>();
    public List<CustomerRequest> customerRequests = new List<CustomerRequest>();
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();
    public List<ARClickHandler.ButtonType> userActions = new List<ARClickHandler.ButtonType>();

    private string selectedStrength = "";
    private int selectedTime = 0;
    private string selectedTimeType = "";

    private string sessionDocId => session.UserId + "_session";

    private float requestStartTime;
    private int currentRequestIndex = 0;
    private float timeRemaining;
    private bool isTimerRunning = false;
    private int taskCounter = 0;
    public GameObject arrowForStrengthButton;
    public GameObject arrowForBrewButton;
    public GameObject arrowForFreshButton;
    public GameObject arrowForCleanButton;
    public GameObject arrowForTimeIncreaseButton;
    private int currentInstructionIndex = 0;
    private GameObject[] instructionArrows;
    private List<ARClickHandler.ButtonType> userActionsBackup = new List<ARClickHandler.ButtonType>();
    private int instructionIndexBackup = 0;
    private List<TaskEvaluationResult> evaluationResults = new List<TaskEvaluationResult>();
    private int errors;
    private int correctTasks;
    private float totalTimeRemaining = 0f;



    void Awake()
    {
        firestore = FirebaseManager.Instance.Firestore;
        auth = FirebaseManager.Instance.Auth;
        session = UserSession.Instance;
        session.LoadFromPrefs();
        chatGPT = FindObjectOfType<ChatGPT>();
        if (chatGPT == null)
            Debug.LogWarning("ChatGPT object not found. AI feedback will not be available.");
    }

    void Start()
    {
        SetupCoffeeRecipes();
        SetupCustomerRequests();
        instructionArrows = new GameObject[]
    {
        arrowForStrengthButton,      // 0
        arrowForTimeIncreaseButton,  // 1
        arrowForBrewButton,          // 2
        arrowForFreshButton,         // 3
        arrowForCleanButton          // 4
    };
        DisplayCurrentRequest();
        ListenToTrainerHints();
    }

    void SetupCoffeeRecipes()
    {
        coffeeRecipes.Add(new CoffeeRecipe("Espresso", "High", 25, "Short", new List<ARClickHandler.ButtonType> { ARClickHandler.ButtonType.Strength, ARClickHandler.ButtonType.Brew }, "Small, strong shot"));
        coffeeRecipes.Add(new CoffeeRecipe("Americano", "Medium", 30, "Medium", new List<ARClickHandler.ButtonType> { ARClickHandler.ButtonType.Strength, ARClickHandler.ButtonType.Fresh, ARClickHandler.ButtonType.Brew }, "Add water first (Fresh)"));
        coffeeRecipes.Add(new CoffeeRecipe("Latte", "Medium", 90, "Long", new List<ARClickHandler.ButtonType> { ARClickHandler.ButtonType.Strength, ARClickHandler.ButtonType.Brew }, "Milk added separately"));
        coffeeRecipes.Add(new CoffeeRecipe("Drip Coffee", "Low", 180, "Long", new List<ARClickHandler.ButtonType> { ARClickHandler.ButtonType.Strength, ARClickHandler.ButtonType.Brew }, "Classic style"));
        coffeeRecipes.Add(new CoffeeRecipe("Clean Cycle", "", 60, "", new List<ARClickHandler.ButtonType> { ARClickHandler.ButtonType.Clean }, "Maintenance"));
    }

    public void OnTimeIncrease()
    {
        selectedTime += 5;
        UpdateTimerUI();
    }

    public void OnTimeDecrease()
    {
        if (selectedTime > 0)
            selectedTime -= 5;
        UpdateTimerUI();
    }

    public void OnStrengthSelected(string strength)
    {
        selectedStrength = strength;

        if (strengthText != null)
            strengthText.text = $"Strength: {strength}";
    }


    public void OnTimeTypeSelected(string timeType)
    {
        selectedTimeType = timeType;
    }

    public void OnButtonPressed(ARClickHandler.ButtonType button)
    {
        if (!pressedButtons.Contains(button))
            pressedButtons.Add(button);
    }

    public CoffeeRecipe MatchRecipe()
    {
        foreach (var recipe in coffeeRecipes)
        {
            if (recipe.strength == selectedStrength &&
                recipe.requiredTime == selectedTime &&
                recipe.timeType == selectedTimeType &&
                recipe.requiredButtons.TrueForAll(b => pressedButtons.Contains(b)) &&
                pressedButtons.TrueForAll(b => recipe.requiredButtons.Contains(b)))
            {
                return recipe;
            }
        }
        return null;
    }

    void UpdateTimerUI()
    {
        timerText.text = selectedTime.ToString() + "s";

        if (coffeeTimeText != null)
            coffeeTimeText.text = "Time: " + selectedTime.ToString() + "s";
    }

    void ListenToTrainerHints()
    {
        firestore.Collection("liveSessions")
            .Document(sessionDocId)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists) return;

                if (snapshot.ContainsField("hint"))
                {
                    string hint = snapshot.GetValue<string>("hint");

                    if (!string.IsNullOrEmpty(hint))
                        trainerHintText.text = "Trainer Hint: " + hint;
                    else
                        trainerHintText.text = "";
                }
            });
    }

    void UpdateLiveSessionStatus(string statusMessage)
    {
        if (session == null || string.IsNullOrEmpty(session.UserId)) return;

        var data = new Dictionary<string, object>
        {
            { "module", "coffeeMachine" },
            { "timestamp", Timestamp.GetCurrentTimestamp() },
            { "lastAction", statusMessage },
            { "isActive", true }
        };

        firestore.Collection("liveSessions")
            .Document(sessionDocId)
            .SetAsync(data, SetOptions.MergeAll);
    }

    void UpdateLiveSession(string moduleName, int activeStep, string hint)
    {
        if (session == null || string.IsNullOrEmpty(session.UserId))
        {
            Debug.LogError("Invalid session — UserId is null or empty. Skipping session update.");
            return;
        }

        Dictionary<string, object> sessionData = new Dictionary<string, object>
        {
            { "traineeId", session.UserId },
            { "trainerId", FieldValue.Delete },
            { "module", moduleName },
            { "activeStep", activeStep },
            { "hint", hint },
            { "requestText", customerRequestText.text },
            { "instructions", instructionsText.text },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        firestore.Collection("liveSessions")
            .Document(sessionDocId)
            .SetAsync(sessionData, SetOptions.MergeAll);
    }

    public void NotifyTrainer(string eventName)
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "animationStep", eventName },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        firestore.Collection("liveSessions")
            .Document(sessionDocId)
            .UpdateAsync(data);
    }
    public void OnTaskCompleted()
    {
        float timeTaken = Time.time - requestStartTime;
        float requestTimeLimit = customerRequests[currentRequestIndex].timeLimit;
        float timeRemainingForTask = Mathf.Max(0, requestTimeLimit - timeTaken);
        totalTimeRemaining += timeRemainingForTask;

        Debug.Log("Task completed.");

        CoffeeRecipe matchedRecipe = MatchRecipe();
        string result = matchedRecipe != null
            ? $"Success! You made a {matchedRecipe.name}. {matchedRecipe.notes}"
            : "Task completed, but the recipe did not match any known coffee type.";

        evaluationResults.Add(new TaskEvaluationResult(
            customerRequests[currentRequestIndex].requestText,
            matchedRecipe != null,
            matchedRecipe != null ? matchedRecipe.name : "No match",
            timeTaken,
            pressedButtons
        ));




        currentRequestIndex++;
        if (currentRequestIndex >= customerRequests.Count)
        {
            Debug.Log("All tasks completed.");
            bool lastWasClean = customerRequests[customerRequests.Count - 1].requestText.Contains("Clean");

            if (lastWasClean)
                StartCoroutine(ShowCleaningTextThenReport());
            else
                StartCoroutine(GenerateSummaryAndShowReport());

            return;
        }
        if (currentRequestIndex < customerRequests.Count)
        {
            // More requests to handle
            ResetInputs();
            DisplayCurrentRequest();
        }
        else
        {
            // If the last task was "Run a Clean Cycle", show cleaning message first
            if (customerRequests[currentRequestIndex - 1].requestText.Contains("Clean"))
            {
                StartCoroutine(ShowCleaningTextThenReport());
            }
            else
            {
                StartCoroutine(GenerateSummaryAndShowReport());
            }
        }
    }


    void SetupCustomerRequests()
    {
        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make an Espresso",
            timeLimit = 25,
            instructions = new string[] { "Set strength to High", "Set time to 25s", "Press Brew" },
            expectedActions = new ARClickHandler.ButtonType[] {
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.Brew
        }
        });

        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make an Americano",
            timeLimit = 30,
            instructions = new string[] { "Set strength to Medium", "Add water (Fresh)", "Set time to 30s", "Press Brew" },
            expectedActions = new ARClickHandler.ButtonType[] {
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.Fresh,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.Brew
        }
        });

        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make a Latte",
            timeLimit = 60,
            instructions = new string[] { "Set strength to Medium", "Set time to 60s", "Press Brew" },
            expectedActions = new ARClickHandler.ButtonType[] {
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.Brew
        }
        });

        customerRequests.Add(new CustomerRequest
        {
            requestText = "Make a Drip Coffee",
            timeLimit = 180,
            instructions = new string[] { "Set strength to Low", "Set time to 180s", "Press Brew" },
            expectedActions = new ARClickHandler.ButtonType[] {
            ARClickHandler.ButtonType.Strength,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.TimeIncrease,
            ARClickHandler.ButtonType.Brew
        }
        });

        /*customerRequests.Add(new CustomerRequest
        {
            requestText = "Run a Clean Cycle",
            timeLimit = 20,
            instructions = new string[] { "Press Clean to start maintenance cycle" },
            expectedActions = new ARClickHandler.ButtonType[] {
            ARClickHandler.ButtonType.Clean
        }
        });*/
    }

    void DisplayCurrentRequest()
    {
        if (customerRequests.Count == 0 || currentRequestIndex >= customerRequests.Count)
            return;

        CustomerRequest request = customerRequests[currentRequestIndex];
        customerRequestText.text = request.requestText;
        instructionsText.text = string.Join("\n", request.instructions);

        selectedTime = 0; // Let user increase time
        UpdateTimerUI();
        requestStartTime = Time.time;
        currentInstructionIndex = 0;
        UpdateInstructionArrow();
        StartCountdown(); // Start the countdown here
    }

    public void StartCountdown()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        int requestTimeLimit = 0;
        if (customerRequests.Count > currentRequestIndex)
            requestTimeLimit = (int)customerRequests[currentRequestIndex].timeLimit;

        countdownCoroutine = StartCoroutine(Countdown(requestTimeLimit));
    }


    private IEnumerator Countdown(int requestTimeLimit)
    {
        int remainingTime = requestTimeLimit;
        while (remainingTime > 0)
        {
            timerText.text = remainingTime + "s";
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }

        timerText.text = "0s";
        NotifyTrainer("TimeUp");


    }
    public void ShowFinalReport(string feedback)
    {
        isTimerRunning = false;
        finalReportPanel.SetActive(true);
        finalReportText.gameObject.SetActive(true);
        finalReportText.text = "Session Complete!\n\n" + feedback;
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
    private IEnumerator HideErrorMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        errorMessageText.gameObject.SetActive(false);
    }

    public bool RegisterButtonClick(ARClickHandler.ButtonType clickedButton)
    {
        var currentRequest = customerRequests.Find(r => r.requestText == customerRequestText.text);
        if (currentRequest == null)
        {
            Debug.LogWarning("Request not found.");
            ShowErrorMessage("Request error.");
            return false;
        }

        userActionsBackup = new List<ARClickHandler.ButtonType>(userActions);
        instructionIndexBackup = currentInstructionIndex;

        userActions.Add(clickedButton);
        UpdateLiveSessionStatus($"Clicked: {clickedButton}");

        if (userActions.Count > currentRequest.expectedActions.Length)
        {
            ShowErrorMessage("Too many steps!");
            UpdateLiveSessionStatus("Too many steps");
            return false;
        }

        for (int i = 0; i < userActions.Count; i++)
        {
            if (userActions[i] != currentRequest.expectedActions[i])
            {
                errors++;
                ShowErrorMessage("Wrong action!");
                UpdateLiveSessionStatus("Wrong action");
                return false;
            }
        }
        currentInstructionIndex++;
        correctTasks++;
        UpdateInstructionArrow();
        UpdateLiveSession(currentRequest.requestText, userActions.Count, "");
        return true;
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
        userActions = new List<ARClickHandler.ButtonType>(userActionsBackup);
        currentInstructionIndex = instructionIndexBackup;
        UpdateInstructionArrow();
        isTimerRunning = true;
    }
    public void GoToNextRequest()
    {
        currentRequestIndex++;
        userActions.Clear();
        selectedStrength = "";
        selectedTime = 0;
        selectedTimeType = "";
        pressedButtons.Clear();

        if (currentRequestIndex < customerRequests.Count)
        {
            finalReportPanel.SetActive(false);
            DisplayCurrentRequest();
        }
        else
        {
            ShowFinalReport("All tasks completed!");
        }
    }
    void ResetInputs()
    {
        selectedStrength = "";
        selectedTime = 0;
        selectedTimeType = "";
        pressedButtons.Clear();
        userActions.Clear();
        UpdateTimerUI();

        ARClickHandler[] allHandlers = FindObjectsOfType<ARClickHandler>();
        foreach (var handler in allHandlers)
        {
            handler.ResetStrengthLevel();
        }
    }

     /*public IEnumerator GenerateAISummaryAndShowReport()
     {
         if (chatGPT == null)
         {
             ShowFinalReport("No AI feedback available.");
             yield break;
         }

         // Build a summary string from all customer request results
         string performanceSummary = "";
         foreach (var result in taskResults)
         {
             string request = result.ContainsKey("request") ? result["request"].ToString() : "Unknown Request";
             string outcome = result.ContainsKey("result") ? result["result"].ToString() : "No result";
             string time = result.ContainsKey("timeTaken") ? result["timeTaken"].ToString() + "s" : "N/A";

             performanceSummary += $"Request: {request}\nResult: {outcome}\nTime Taken: {time}\n\n";
         }

         bool reportShown = false;

         // Use your ChatGPT script's method
         yield return chatGPT.SendPerformanceSummary(performanceSummary, (aiFeedback) =>
         {
             ShowFinalReport(aiFeedback);
             reportShown = true;
         });

         // Fallback safety
         if (!reportShown)
             ShowFinalReport("Task completed, but failed to generate AI feedback.");
     } */
    public IEnumerator GenerateSummaryAndShowReport()
    {
        finalReportPanel.SetActive(true);
        finalReportText.gameObject.SetActive(true);

        int totalTasks = taskResults.Count;

        float totalTime = 0f;
        List<string> mistakeList = new List<string>();

        for (int i = 0; i < totalTasks; i++)
        {
            var result = taskResults[i];
            string request = result.ContainsKey("request") ? result["request"].ToString() : $"Task {i + 1}";
            string outcome = result.ContainsKey("result") ? result["result"].ToString() : "No result";
            float time = 0f;

            if (result.ContainsKey("timeTaken"))
            {
                if (result["timeTaken"] is float f)
                    time = f;
                else if (float.TryParse(result["timeTaken"].ToString(), out float parsed))
                    time = parsed;
            }

            totalTime += time;

            if (outcome.StartsWith("Matched"))
            {
                correctTasks++;
            }
            else
            {
                mistakeList.Add($"• {request}: {outcome}");
            }
        }


        string summary = $"<b>Training Summary</b>\n\n";
        summary += $"Correct Tasks: {correctTasks}\n";
        summary += $"Mistakes Made: {errors}\n";

        float avgRemainingTime = (totalTasks > 0) ? totalTimeRemaining / totalTasks : 0f;
        summary += $"Average Time Taken: {Mathf.RoundToInt(avgRemainingTime)}s";

        if (avgRemainingTime >= 10)
            summary += "\nGreat timing!";
        else
            summary += "\n Try to complete tasks with more time left.";


        yield return new WaitForSeconds(1f);
        finalReportText.text = summary;

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            finalReportPanel.SetActive(false);
            finalReportText.gameObject.SetActive(false);
            continueButton.gameObject.SetActive(false);
            GoToNextRequest();
        });
    }





    public IEnumerator ShowCleaningTextThenReport()
    {
        OnTaskCompleted();
        finalReportPanel.SetActive(true);
        finalReportText.gameObject.SetActive(true);
        instructionsText.text = "Cleaning in progress... Please wait.";
        yield return new WaitForSeconds(3f);
        StartCoroutine(GenerateSummaryAndShowReport());
    }

    void UpdateInstructionArrow()
    {
        foreach (var arrow in instructionArrows)
            arrow.SetActive(false);

        if (currentRequestIndex < customerRequests.Count)
        {
            var request = customerRequests[currentRequestIndex];
            if (currentInstructionIndex < request.expectedActions.Length)
            {
                var currentAction = request.expectedActions[currentInstructionIndex];

                switch (currentAction)
                {
                    case ARClickHandler.ButtonType.Strength:
                        arrowForStrengthButton.SetActive(true); break;
                    case ARClickHandler.ButtonType.TimeIncrease:
                        arrowForTimeIncreaseButton.SetActive(true); break;
                    case ARClickHandler.ButtonType.Brew:
                        arrowForBrewButton.SetActive(true); break;
                    case ARClickHandler.ButtonType.Fresh:
                        arrowForFreshButton.SetActive(true); break;
                    case ARClickHandler.ButtonType.Clean:
                        arrowForCleanButton.SetActive(true); break;
                }
            }
        }
    }


}