using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreCalculator : MonoBehaviour
{
    int correctAnswers = 0;
    int questionCompleted = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public int CalculateScore()
    {
        if (questionCompleted == 0) return 0;
        float percentageCorrect = ((float)correctAnswers / questionCompleted) *100;
        return Mathf.RoundToInt(percentageCorrect);
    }
    public void UpdateCorrectAnswers()
    {
        correctAnswers++;
    }
    public void UpdateQuestionCompleted()
    {
        questionCompleted++;
    }
}
