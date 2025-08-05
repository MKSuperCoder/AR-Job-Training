using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Quiz: MonoBehaviour
{
    [SerializeField] GameObject endScreen;
    [SerializeField] GameObject quizScreen;
    [SerializeField] TMP_Text questionText;
    [SerializeField] QuestionSO[] questions;
    [SerializeField] Button[] answerButtons;
    [SerializeField] Button nextButton;
    ScoreCalculator scoreCalculator;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] Slider progressBar;
    int questionIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        scoreCalculator = FindFirstObjectByType<ScoreCalculator>();
        Debug.Log("scoreCalculator: " + scoreCalculator);
        DisplayQuestion();
        SetButtonState(true);
        progressBar.maxValue = questions.Length;
        progressBar.value = 0;
    }

    public void DisplayQuestion()
    {
        SetButtonState(true);
        questionText.text = questions[questionIndex].GetQuestion();
        for (int i = 0; i < answerButtons.Length; i++)
        {
            TMP_Text buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();
            buttonText.text = questions[questionIndex].GetAnswer(i);
        }
    }
    public void OnAnswerSelected(int index)
    {
        scoreCalculator.UpdateQuestionCompleted();
        if (index == questions[questionIndex].GetCorrectAnswerIndex())
        {
            questionText.text = "Correct";
            scoreCalculator.UpdateCorrectAnswers();
        }
        else
        {
            questionText.text = "Incorrect";
        }
        SetButtonState(false);
        scoreText.text = "Score: " + scoreCalculator.CalculateScore() + "%";
        progressBar.value++;
    }
    void SetButtonState(bool state)
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = state;
        }
    }
    public void IncreaseQuestionIndex()
    {
        questionIndex++;
    }
    public void EnableNextButon()
    {
        if (questionIndex == questions.Length - 1)
        {
            nextButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(true);
        }
    }
    public bool IsLastQuestionAnswered()
    {
        return questionIndex >= questions.Length - 1 && !answerButtons[0].interactable;
    }
    // Update is called once per frame
    void Update()
    {
       if (IsLastQuestionAnswered())
        {
            endScreen.gameObject.SetActive(true);
            quizScreen.gameObject.SetActive(false);
        }
    }
}
