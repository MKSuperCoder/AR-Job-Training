[System.Serializable]
public class CustomerRequest
{
    public string requestText;
    public float timeLimit;
    public string[] instructions;
    public ARClickHandler.ButtonType[] expectedActions;  
}
