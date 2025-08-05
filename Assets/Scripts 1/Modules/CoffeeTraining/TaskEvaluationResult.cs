using System.Collections.Generic;

public class TaskEvaluationResult
{
    public string RequestName;
    public bool Matched;
    public string MatchedRecipe;
    public float TimeTaken;
    public List<ARClickHandler.ButtonType> PressedButtons;

    public TaskEvaluationResult(string requestName, bool matched, string matchedRecipe, float timeTaken, List<ARClickHandler.ButtonType> pressedButtons)
    {
        RequestName = requestName;
        Matched = matched;
        MatchedRecipe = matchedRecipe;
        TimeTaken = timeTaken;
        PressedButtons = new List<ARClickHandler.ButtonType>(pressedButtons); // Deep copy
    }
}
