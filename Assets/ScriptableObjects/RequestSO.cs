using UnityEngine;

[CreateAssetMenu(menuName = "Coffee Request", fileName = "Request")]
public class RequestSO : ScriptableObject
{
    [TextArea(2, 6)]
    [SerializeField] private string request = "Enter new request here";

    public string[] stepTargetNames; // Use string names instead of GameObjects

    public string GetRequest()
    {
        return request;
    }

    public string[] GetStepTargetNames()
    {
        return stepTargetNames;
    }
}
