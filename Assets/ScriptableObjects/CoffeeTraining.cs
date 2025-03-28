using TMPro;
using UnityEngine;

public class CoffeeTraining : MonoBehaviour
{
    [SerializeField] RequestSO[] requests;
    [SerializeField] TMP_Text requestText;
    [SerializeField] MoveArrow arrow;

    int requestIndex = 0;
    int stepIndex = 0;
    RequestSO currentRequest;

    void Start()
    {
        DisplayRequest();
    }

    public void DisplayRequest()
    {
        currentRequest = requests[requestIndex];
        requestText.text = currentRequest.GetRequest();
        stepIndex = 0;
        MoveToNextStep();
    }

    public void MoveToNextStep()
    {
        if (stepIndex < currentRequest.GetSteps().Length)
        {
            arrow.PointTo(currentRequest.GetSteps()[stepIndex]);
        }
        else
        {
            Debug.Log("Request complete!");
            // Optionally go to the next request or end
        }
    }

    public void OnCorrectButtonPressed()
    {
        stepIndex++;
        MoveToNextStep();
    }
}
