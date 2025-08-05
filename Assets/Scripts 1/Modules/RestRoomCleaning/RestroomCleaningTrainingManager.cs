using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class RestroomCleaningTrainingManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text instructionText;
    public TMP_Text taskText;
    public TMP_Text countdownText;
    public TMP_Text levelText;
    public TMP_Text mopInstructionText;
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;


    [Header("Buttons")]
    public Button wearGlovesButton;
    public Button applyCleanerButton;
    public Button getTowelButton;

    [Header("Objects")]
    public GameObject imageGuide;
    public GameObject flushHandle;
    public List<GameObject> messObjects;
    public List<GameObject> areaNotMopped;
    public GameObject toiletBrush;
    public GameObject towel;
    public GameObject mop;
    public List<GameObject> tissuePapers;
    public Transform trashBasket;
    public GameObject mopFloorInstructions;

    [Header("Towel Spawn")]
    public GameObject towelPrefab;
    public Transform towelSpawnPoint;

    [Header("Task System")]
    public RestroomLevel currentLevel;  // Set in Inspector
    private int currentTaskIndex = 0;
    private float timeRemaining;
    private bool timerRunning = false;

    private GameObject spawnedTowel;
    private bool glovesOn = false;
    private bool cleanerApplied = false;
    private int messCleaned = 0;
    private int tissuesDropped = 0;
    private int areaMopped = 0;
    public GameObject arrow1;
    public GameObject arrow2;
    public GameObject arrow3;
    public GameObject arrow4;
    public GameObject arrow5;
    private List<string> mistakes = new List<string>();


    void Start()
    {
        wearGlovesButton.onClick.AddListener(WearGloves);
        applyCleanerButton.onClick.AddListener(ApplyCleaner);
        getTowelButton.onClick.AddListener(SpawnTowel);

        LoadTask(0); // Start first task from currentLevel
    }

    void Update()
    {
        if (timerRunning)
        {
            timeRemaining -= Time.deltaTime;
            countdownText.text = "Time Left: " + Mathf.Ceil(timeRemaining).ToString() + "s";

            if (timeRemaining <= 0)
            {
                timerRunning = false;
                instructionText.text = "⏱ Time’s up! Try again or proceed.";
            }
        }
    }

    void LoadTask(int index)
    {
        if (index >= currentLevel.tasks.Count)
        {
            LevelComplete();
            return;
        }

        currentTaskIndex = index;
        RestroomTasks task = currentLevel.tasks[index];

        levelText.text = "Level " + currentLevel.levelName;
        taskText.text = task.taskText;
        instructionText.text = task.instructions.Length > 0 ? task.instructions[0] : "";
        timeRemaining = task.timeLimit;
        timerRunning = true;
    }

    public void CompleteCurrentTask()
    {
        timerRunning = false;
        currentTaskIndex++;
        LoadTask(currentTaskIndex);
    }

    void LevelComplete()
    {
        instructionText.text = "All tasks complete!";
        levelText.text = "Level " + currentLevel.levelName + " - Complete";
        countdownText.text = "";
        timerRunning = false;

        // Load next scene after short delay
        ShowFeedback();
    }

    void WearGloves()
    {
        glovesOn = true;
        instructionText.text = "Gloves on. Now tap the flush handle.";
        flushHandle.SetActive(true);
        arrow1.SetActive(true);
    }

    void SpawnTowel()
    {
        if (spawnedTowel != null)
            Destroy(spawnedTowel);

        Vector3 spawnPosition = Camera.main.transform.position + Camera.main.transform.forward * 1.5f;
        Quaternion spawnRotation = Quaternion.LookRotation(Camera.main.transform.forward);
        spawnedTowel = Instantiate(towelPrefab, spawnPosition, spawnRotation);
        EnableDragging(spawnedTowel);
        instructionText.text = "Use the towel to clean the seat and lid.";
    }

    public void OnFlushHandleClicked()
    {
        if (!glovesOn)
        {
            LogMistake("Flushed toilet without wearing gloves.");
            return;
        }
        flushHandle.SetActive(false);
        applyCleanerButton.gameObject.SetActive(true);
        instructionText.text = "Toilet flushed. Now apply cleaner.";
        CompleteCurrentTask();
        arrow1.SetActive(false);
        
    }

    void ApplyCleaner()
    {
        if (!glovesOn)
        {
            LogMistake("Applied cleaner without gloves.");
            return;
        }
        cleanerApplied = true;
        applyCleanerButton.gameObject.SetActive(false);
        instructionText.text = "Cleaner applied. Now scrub inside bowl with brush.";
        EnableDragging(toiletBrush);
        arrow2.SetActive(true);
        arrow3.SetActive(true);
    }

    public void OnToiletBrushed()
    {
        imageGuide.SetActive(false);
        instructionText.text = "Now clean seat and lid using the towel.";
        EnableDragging(towel);
        HighlightMessSpots();
        CompleteCurrentTask();
        arrow2.SetActive(false);
        arrow3.SetActive(false);
    }

    public void OnMessCleaned(GameObject mess)
    {
        mess.SetActive(false);
        messCleaned++;

        if (messCleaned >= messObjects.Count)
        {
            instructionText.text = "Great! Now pick up the tissues and drop them in the bin.";
            EnableDraggingTissues();
            CompleteCurrentTask();
            arrow4.SetActive(true);
        }
    }

    public void OnTissueDroppedInBin()
    {
        tissuesDropped++;

        if (tissuesDropped >= tissuePapers.Count)
        {
            instructionText.text = "Now mop the floor!";
            mopFloorInstructions.SetActive(true);
            mopInstructionText.gameObject.SetActive(true);
            EnableDragging(mop);
            CompleteCurrentTask();
            arrow4.SetActive(false);
            arrow5.SetActive(true);
        }
    }

    public void OnFloorMopped(GameObject mopArea)
    {
        mopArea.SetActive(false);
        areaMopped++;

        if (areaMopped >= areaNotMopped.Count)
        {
            mopInstructionText.text = "All areas mopped.";
            instructionText.text = "Restroom cleaned successfully!";
            levelText.text = "Toilet Level Complete!";
            timerRunning = false;
            CompleteCurrentTask();
            arrow5.SetActive(false);
        }
    }

    void EnableDragging(GameObject item)
    {
        var drag = item.GetComponent<Draggable>();
        if (drag != null)
            drag.enabled = true;
    }

    void HighlightMessSpots()
    {
        foreach (var mess in messObjects)
            mess.SetActive(true);
    }

    void EnableDraggingTissues()
    {
        foreach (var tissue in tissuePapers)
            EnableDragging(tissue);
    }
    void LoadNextScene()
    {
        SceneManager.LoadScene("RestroomCleaningLevel2");
    }
    public void LogMistake(string message)
    {
        mistakes.Add(message);
    }
    void ShowFeedback()
    {
        if (feedbackPanel != null && feedbackText != null)
        {
            feedbackPanel.SetActive(true);

            string feedback = "Restroom Training Complete!\n\n";

            if (mistakes.Count == 0)
            {
                feedback += "No mistakes made!\nGreat attention to hygiene steps!";
            }
            else
            {
                feedback += $"Mistakes: {mistakes.Count}\n";
                foreach (string mistake in mistakes)
                    feedback += "• " + mistake + "\n";

                feedback += "\nTips: Always follow hygiene order. Re-read instructions. Practice again.";
            }

            feedbackText.text = feedback;

            // Optionally delay next scene
            Invoke("LoadNextScene", 5f);
        }
    }

}
