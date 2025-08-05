using System.Collections.Generic;

[System.Serializable]
public class CoffeeRecipe
{
    public string name;
    public string strength;
    public int requiredTime;
    public string timeType;
    public List<ARClickHandler.ButtonType> requiredButtons;
    public string notes;

    public CoffeeRecipe(string name, string strength, int requiredTime, string timeType, List<ARClickHandler.ButtonType> buttons, string notes)
    {
        this.name = name;
        this.strength = strength;
        this.requiredTime = requiredTime;
        this.timeType = timeType;
        this.requiredButtons = buttons;
        this.notes = notes;
    }
}