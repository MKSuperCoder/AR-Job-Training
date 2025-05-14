using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARFoodPlacementManager : MonoBehaviour
{
    public GameObject highlightBurger;
    public GameObject highlightFries;
    public GameObject highlightDrink;

    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    private GameObject selectedObject;
    private Vector2 touchPosition;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool highlightPlaced = false;
    private Transform highlightParent; // Base reference point for highlight positions

    public TMP_Text taskText;
    public TMP_Text instructionsText;
    public GameObject finalReportPanel;
    public TMP_Text finalReportText;

    public ChatGPT chatGPT;

    private List<FoodPlacementTask> tasks = new List<FoodPlacementTask>();
    private List<Dictionary<string, object>> taskResults = new List<Dictionary<string, object>>();
    private int currentTaskIndex = 0;
    private GameObject lastPlacedObject;


    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();

        // Hide highlights initially
        highlightBurger.SetActive(false);
        highlightFries.SetActive(false);
        highlightDrink.SetActive(false);
        SetupTasks();
        DisplayCurrentTask();

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

        // Add more tasks...
    }


    void Update()
    {
        if (!highlightPlaced)
        {
            TryPlaceHighlightBase();
            return;
        }

        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        touchPosition = touch.position;

        if (touch.phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider != null)
            {
                if (hit.collider.CompareTag("Draggable"))
                {
                    selectedObject = hit.collider.gameObject;
                    ShowHighlightFor(selectedObject);
                }
            }
        }

        if (touch.phase == TouchPhase.Moved && selectedObject != null)
        {
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                selectedObject.transform.position = hitPose.position;
            }
        }

        if (touch.phase == TouchPhase.Ended)
        {
            if (selectedObject != null)
            {
                HideAllHighlights();
                selectedObject = null;
            }
        }
    }

    void TryPlaceHighlightBase()
    {
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            // Place highlights at slightly different nearby positions
            highlightBurger.transform.position = pose.position + new Vector3(-0.2f, 0f, 0.2f);
            highlightFries.transform.position = pose.position + new Vector3(0.0f, 0f, 0.2f);
            highlightDrink.transform.position = pose.position + new Vector3(0.2f, 0f, 0.2f);

            highlightPlaced = true;
        }
    }


    void ShowHighlightFor(GameObject item)
    {
        HideAllHighlights();

        if (item.name.Contains("Burger"))
            highlightBurger.SetActive(true);
        else if (item.name.Contains("Fries"))
            highlightFries.SetActive(true);
        else if (item.name.Contains("FountainCup"))
            highlightDrink.SetActive(true);
    }

    void HideAllHighlights()
    {
        highlightBurger.SetActive(false);
        highlightFries.SetActive(false);
        highlightDrink.SetActive(false);
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

}
