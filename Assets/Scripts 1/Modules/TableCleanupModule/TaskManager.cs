using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TaskManager : MonoBehaviour
{
    public TMP_Text stepText;

    [Header("Task Sets")]
    public GameObject task1Objects;
    public GameObject task2Objects;

    [Header("Task 1 Highlighters")]
    public GameObject burgerBoxHighlighter;
    public GameObject strawHighlighter;
    public GameObject friesBoxHighlighter;
    public GameObject cupHighlighter;

    [Header("Task 2 Highlighters")]
    public GameObject nuggetHighlighter1;
    public GameObject nuggetHighlighter2;
    public GameObject nuggetHighlighter3;
    public GameObject nuggetHighlighter4;
    public GameObject nuggetHighlighter5;
    public GameObject nuggetHighlighter6;
    public GameObject cupHighlighter2;
    public GameObject spilledDrinkHighlighter;
    public GameObject nuggetBoxHighlighter;
    public GameObject friesBoxHighlighter2;

    [Header("Feedback UI")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;

    public ModuleManager moduleManager;
    private int currentTask = 1;
    private int currentStep = 0;

    private readonly List<string> task1Steps = new List<string> {
        "Cup", "Straw", "Burger Box", "Fries Box"
    };

    private readonly List<string> task2Steps = new List<string>
    {
        "Clean Nuggets",         // Step 1
        "Clean Spilled Drink",   // Step 2
        "Place Cup",             // Step 3
        "Place Nugget Box",      // Step 4
        "Place Fries Box"        // Step 5
    };


    private bool isCheckingMess = false;

    void Start()
    {
        task1Objects.SetActive(true);
        task2Objects.SetActive(false);
        UpdateStepUI();
    }
    void Update()
    {
        if (currentTask == 2)
        {
            string expectedStep = task2Steps[currentStep];

            // Continuously check if all spilled drinks are cleaned
            if (expectedStep == "Clean Spilled Drink" && GameObject.FindGameObjectsWithTag("Liquid").Length == 0)
            {
                currentStep++;
                UpdateStepUI();
            }
        }
    }


    public void StepCompleted(string objectName)
    {
        string cleanedName = objectName.Replace("(Clone)", "").Trim();

        if (currentTask == 1)
        {
            if (currentStep < task1Steps.Count &&
                task1Steps[currentStep].Equals(cleanedName, System.StringComparison.OrdinalIgnoreCase))
            {
                currentStep++;
                if (currentStep >= task1Steps.Count)
                {
                    ProceedToTask2();
                }
                else
                {
                    UpdateStepUI();
                }
            }
        }
        else if (currentTask == 2)
        {
            HandleTask2Step(cleanedName);
        }
    }

    public void HandleTask2Step(string cleanedName)
    {
        if (currentStep >= task2Steps.Count) return;

        string expectedStep = task2Steps[currentStep];

        // Step: Clean Nuggets
        if (expectedStep == "Clean Nuggets")
        {
            if (!isCheckingMess)
                StartCoroutine(CheckMessCleared());
            return;
        }

        // Step: Clean Spilled Drink
        if (expectedStep == "Clean Spilled Drink")
        {
            if (GameObject.FindGameObjectsWithTag("Liquid").Length == 0)
            {
                currentStep++;
                UpdateStepUI();
            }
            return;
        }

        // Step: Place objects (check cleanedName against expectedStep)
        if (expectedStep.StartsWith("Place "))
        {
            string expectedObject = expectedStep.Replace("Place ", "");
            if (expectedObject.Equals(cleanedName, System.StringComparison.OrdinalIgnoreCase))
            {
                currentStep++;
                if (currentStep < task2Steps.Count)
                    UpdateStepUI();
                else
                    ShowFeedback();
            }
        }
    }


    IEnumerator CheckMessCleared()
    {
        isCheckingMess = true;
        while (GameObject.FindGameObjectsWithTag("Mess").Length > 0)
        {
            yield return null;
        }
        currentStep++;
        UpdateStepUI();
        isCheckingMess = false;
    }

    void UpdateStepUI()
    {
        if (stepText == null) return;

        if (currentTask == 1 && currentStep < task1Steps.Count)
        {
            stepText.text = $"Step {currentStep + 1}: Place the {task1Steps[currentStep]} on the tray";

            cupHighlighter.SetActive(currentStep == 0);
            strawHighlighter.SetActive(currentStep == 1);
            burgerBoxHighlighter.SetActive(currentStep == 2);
            friesBoxHighlighter.SetActive(currentStep == 3);
        }
        else if (currentTask == 2 && currentStep < task2Steps.Count)
        {
            stepText.text = $"Step {currentStep + 1}: {task2Steps[currentStep]}";

            bool nuggets = currentStep == 0;
            nuggetHighlighter1.SetActive(nuggets);
            nuggetHighlighter2.SetActive(nuggets);
            nuggetHighlighter3.SetActive(nuggets);
            nuggetHighlighter4.SetActive(nuggets);
            nuggetHighlighter5.SetActive(nuggets);
            nuggetHighlighter6.SetActive(nuggets);

            spilledDrinkHighlighter.SetActive(currentStep == 1);
            cupHighlighter2.SetActive(currentStep == 2);
            nuggetBoxHighlighter.SetActive(currentStep == 3);
            friesBoxHighlighter2.SetActive(currentStep == 4);
        }
    }

    void ProceedToTask2()
    {
        currentTask = 2;
        currentStep = 0;
        task1Objects.SetActive(false);
        task2Objects.SetActive(true);
        UpdateStepUI();
    }

    public bool IsTask2()
    {
        return currentTask == 2;
    }
    void ShowFeedback()
    {
        stepText.text = "All tasks completed!";

        if (feedbackPanel != null && feedbackText != null)
        {
            feedbackPanel.SetActive(true);

            List<string> mistakeList = moduleManager.GetMistakes();
            string feedback = "Great job completing the training!\n\n";

            if (mistakeList.Count == 0)
            {
                feedback += "No mistakes made!\nKeep practicing to become even faster!";
            }
            else
            {
                feedback += $"Mistakes made: {mistakeList.Count}\n";
                foreach (string mistake in mistakeList)
                {
                    feedback += "• " + mistake + "\n";
                }
                feedback += "\nTry reviewing instructions more carefully and practicing again.";
            }

            feedbackText.text = feedback;
        }
    }


}
