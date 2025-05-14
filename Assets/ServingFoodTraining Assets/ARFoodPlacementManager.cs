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

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();

        // Hide highlights initially
        highlightBurger.SetActive(false);
        highlightFries.SetActive(false);
        highlightDrink.SetActive(false);
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



        if (selectedObject != null)
{
    var task = tasks[currentTaskIndex];
    lastPlacedObject = selectedObject;

    float distance = Vector3.Distance(selectedObject.transform.position, task.expectedPosition);
    bool correctObject = selectedObject.name.Contains(task.expectedObjectName);
    bool correctPosition = distance < 0.1f;

    SaveTaskResult(correctObject && correctPosition);
    HideAllHighlights();
    selectedObject = null;
}
    }

    void TryPlaceHighlightBase()
    {
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            // Create an invisible parent as the anchor point
            GameObject anchorPoint = new GameObject("HighlightAnchor");
            anchorPoint.transform.position = pose.position;
            highlightParent = anchorPoint.transform;

            // Place highlights relative to anchor
            highlightBurger.transform.position = highlightParent.position + new Vector3(-0.1f, 0f, 0.1f);
            highlightFries.transform.position = highlightParent.position + new Vector3(0.0f, 0f, 0.1f);
            highlightDrink.transform.position = highlightParent.position + new Vector3(0.1f, 0f, 0.1f);

            highlightPlaced = true;
        }
    }

    void ShowHighlightFor(GameObject item)
    {
        HideAllHighlights();

        if (item.name.ToLower().Contains("Burger"))
            highlightBurger.SetActive(true);
        else if (item.name.ToLower().Contains("Fries"))
            highlightFries.SetActive(true);
        else if (item.name.ToLower().Contains("FountainCup"))
            highlightDrink.SetActive(true);
    }

    void HideAllHighlights()
    {
        highlightBurger.SetActive(false);
        highlightFries.SetActive(false);
        highlightDrink.SetActive(false);
    }

    void SaveTaskResult(bool success)
{
    var result = new Dictionary<string, object>
    {
        {"task", tasks[currentTaskIndex].taskDescription},
        {"completed", success},
        {"timestamp", System.DateTime.Now.ToString()}
    };

    taskResults.Add(result);

    currentTaskIndex++;
    if (currentTaskIndex >= tasks.Count)
        EndSession();
    else
        DisplayCurrentTask();
}
}
