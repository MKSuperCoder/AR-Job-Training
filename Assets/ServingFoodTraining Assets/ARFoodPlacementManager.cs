using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using Firebase.Firestore;
using Firebase.Auth;
using Firebase.Extensions;

public class ARFoodTrainingManager : MonoBehaviour
{
    public GameObject highlightBurger;
    public GameObject highlightFries;
    public GameObject highlightDrink;

    public TMP_Text taskText;
    public TMP_Text instructionsText;
    public TMP_Text errorMessageText;
    public TMP_Text finalReportText;
    public GameObject finalReportPanel;
    public GameObject errorOverlayPanel;
    public UnityEngine.UI.Button continueButton;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private ChatGPT chatGPT;
    private FirebaseFirestore firestore;
    private FirebaseAuth auth;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject selectedObject;
    private bool highlightPlaced = false;

    private List<FoodPlacementTask> tasks = new List<FoodPlacementTask>();
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();
    private int currentTaskIndex = 0;

    void Awake()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();
        chatGPT = FindObjectOfType<ChatGPT>();
        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
    }

    void Start()
    {
        SetupTasks();
        DisplayCurrentTask();
        HideAllHighlights();
    }

    void Update()
    {
        if (!highlightPlaced)
        {
            TryPlaceHighlightBase();
            return;
        }

        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        Vector2 touchPosition = touch.position;

        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("Draggable"))
            {
                selectedObject = hit.collider.gameObject;
                ShowHighlightFor(selectedObject);
            }
        }

        if (touch.phase == TouchPhase.Moved && selectedObject != null)
        {
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                selectedObject.transform.position = hits[0].pose.position;
            }
        }

        if (touch.phase == TouchPhase.Ended && selectedObject != null)
        {
            EvaluatePlacement(selectedObject);
            selectedObject = null;
        }
    }

    void SetupTasks()
    {
        tasks.Add(new FoodPlacementTask
        {
            taskDescription = "Place the burger at the left position.",
            instructions = new string[] { "Tap and drag the burger.", "Place it on the left highlight." },
            expectedObjectName = "Burger",
            expectedPosition = highlightBurger.transform.position
        });

        tasks.Add(new FoodPlacementTask
        {
            taskDescription = "Place the fries in the center.",
            instructions = new string[] { "Tap and drag the fries.", "Place it on the center highlight." },
            expectedObjectName = "Fries",
            expectedPosition = highlightFries.transform.position
        });
    }

    void DisplayCurrentTask()
    {
        var task = tasks[currentTaskIndex];
        taskText.text = task.taskDescription;
        instructionsText.text = string.Join("\n", task.instructions);
    }

    void TryPlaceHighlightBase()
    {
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;
            highlightBurger.transform.position = pose.position + new Vector3(-0.2f, 0f, 0.2f);
            highlightFries.transform.position = pose.position + new Vector3(0.0f, 0f, 0.2f);
            highlightDrink.transform.position = pose.position + new Vector3(0.2f, 0f, 0.2f);
            highlightPlaced = true;
        }
    }

    void ShowHighlightFor(GameObject item)
    {
        HideAllHighlights();
        if (item.name.Contains("Burger")) highlightBurger.SetActive(true);
        else if (item.name.Contains("Fries")) highlightFries.SetActive(true);
        else if (item.name.Contains("FountainCup")) highlightDrink.SetActive(true);
    }

    void HideAllHighlights()
    {
        highlightBurger.SetActive(false);
        highlightFries.SetActive(false);
        highlightDrink.SetActive(false);
    }

    void EvaluatePlacement(GameObject placedObject)
    {
        var task = tasks[currentTaskIndex];
        float distance = Vector3.Distance(placedObject.transform.position, task.expectedPosition);
        bool correctObject = placedObject.name.Contains(task.expectedObjectName);
        bool correctPosition = distance < 0.1f;

        SaveTaskResult(correctObject && correctPosition);
        HideAllHighlights();
    }

    void SaveTaskResult(bool success)
    {
        FirebaseUser user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("User not logged in.");
            return;
        }

        var result = new Dictionary<string, object>
        {
            {"task", tasks[currentTaskIndex].taskDescription},
            {"completed", success},
            {"timestamp", Timestamp.GetCurrentTimestamp() }
        };

        taskResults.Add(result);

        firestore.Collection("users")
            .Document(user.UserId)
            .Collection("foodPlacementResults")
            .AddAsync(result)
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsCompletedSuccessfully)
                    Debug.LogError("Failed to save: " + task.Exception);
            });

        currentTaskIndex++;
        if (currentTaskIndex >= tasks.Count)
            EndSession();
        else
            DisplayCurrentTask();
    }

    void EndSession()
    {
        string summary = SummarizePerformance(taskResults);
        StartCoroutine(chatGPT.SendPerformanceSummary(summary, ShowFinalReport));
    }

    string SummarizePerformance(List<Dictionary<string, object>> results)
    {
        int completed = 0;
        foreach (var r in results)
            if ((bool)r["completed"]) completed++;

        return $"Completed {completed}/{results.Count} food placements.";
    }

    void ShowFinalReport(string aiFeedback)
    {
        finalReportPanel.SetActive(true);
        finalReportText.text = "Session Complete!\n\n" + aiFeedback;
    }

    public void ShowErrorMessage(string message)
    {
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
    }

    public void OnContinueButtonClicked()
    {
        continueButton.gameObject.SetActive(false);
        errorMessageText.gameObject.SetActive(false);
        errorOverlayPanel.SetActive(false);
    }
}