using TMPro;
using UnityEngine;

public class CoffeeTraining : MonoBehaviour
{
    [SerializeField] RequestSO[] requests;
    [SerializeField] TMP_Text requestText;
    [SerializeField] MoveArrow arrow;

    private int requestIndex = 0;
    private int stepIndex = 0;
    private GameObject[] resolvedStepTargets;
    private RequestSO currentRequest;

    void Start()
    {
        DisplayRequest();
    }

    public void DisplayRequest()
    {
        currentRequest = requests[requestIndex];
        requestText.text = currentRequest.GetRequest();

        // Convert string names to GameObjects
        string[] targetNames = currentRequest.GetStepTargetNames();
        resolvedStepTargets = new GameObject[targetNames.Length];
        for (int i = 0; i < targetNames.Length; i++)
        {
            GameObject found = GameObject.Find(targetNames[i]);
            if (found != null)
                resolvedStepTargets[i] = found;
            else
                Debug.LogError($"Could not find GameObject named '{targetNames[i]}'");
        }

        stepIndex = 0;
        MoveToNextStep();
    }

    public void MoveToNextStep()
    {
        if (stepIndex < resolvedStepTargets.Length)
        {
            arrow.PointTo(resolvedStepTargets[stepIndex]);
        }
        else
        {
            Debug.Log("Request complete!");
            requestText.text = "Good job! Request complete.";
        }
    }

    public void OnCorrectButtonPressed(GameObject buttonPressed)
    {
        GameObject expectedButton = resolvedStepTargets[stepIndex];

        if (buttonPressed == expectedButton)
        {
            stepIndex++;
            MoveToNextStep();
        }
        else
        {
            Debug.Log("Wrong button pressed.");
        }
    }
}
