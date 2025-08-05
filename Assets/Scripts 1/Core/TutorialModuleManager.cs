using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class TutorialModuleManager : MonoBehaviour
{
    public GameObject[] foodObjects;           // List of food objects (e.g., burger, fries, drink...)
    public GameObject[] highlightPoints;       // List of potential highlight positions on table
    public GameObject highlightPrefab;         // Yellow highlighter prefab
    public TMP_Text progressText;              // UI text for progress display (e.g., "1/5")
    public TMP_Text congratulationText;
    public GameObject tutorial1Panel;
    private int currentTaskIndex = 0;
    private int totalTasks;
    private GameObject currentFood;
    private GameObject currentHighlight;
    public Transform startPosition; // drag Position6 here
    private int currentTutorialStage = 1;
    private int currentInstructionIndex = 0;


    void Start()
    {
        totalTasks = foodObjects.Length;
        StartTask();
    }

    void StartTask()
    {
        if (currentTaskIndex >= totalTasks) return;

        // Instantiate food
        currentFood = Instantiate(foodObjects[currentTaskIndex]);
        currentFood.transform.position = startPosition.position;
   

        // Random highlighter position on table
        int rand = Random.Range(0, highlightPoints.Length);
        currentHighlight = Instantiate(highlightPrefab, highlightPoints[rand].transform.position, Quaternion.identity);

        // Set up highlighter trigger
        HighlightTrigger trigger = currentHighlight.AddComponent<HighlightTrigger>();
        trigger.manager = this;

        UpdateProgressText();
    }

    public void CompleteTask()
    {
        Destroy(currentFood);
        Destroy(currentHighlight);
        currentTaskIndex++;
        UpdateProgressText();

        if (currentTaskIndex >= totalTasks)
        {
            if (currentTutorialStage == 1)
            {
                // End of Tutorial 
                congratulationText.text = "Congratulations! You've completed the tutorial.";
                Invoke(nameof(LoadHomePage), 2f);
            }
        }

        StartTask();
    }



    void UpdateProgressText()
    {
        string progress = $"Score: {Mathf.Min(currentTaskIndex, totalTasks)}/{totalTasks}";

        if (currentTutorialStage == 1 && progressText != null)
            progressText.text = progress;
    }

    public void OnObjectPlacedCorrectly(GameObject placedObject, GameObject highlight)
    {
        CompleteTask(); 
    }
    
    public void LoadHomePage(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("New scene has been loaded");
    }
}
