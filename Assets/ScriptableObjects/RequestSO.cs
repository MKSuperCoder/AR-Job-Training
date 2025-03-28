using UnityEngine;

[CreateAssetMenu(menuName = "Coffee Request", fileName = "Request")]
public class RequestSO : ScriptableObject
{
    [TextArea(2,6)]
    [SerializeField] private string request = "Enter new request here";
    public GameObject[] stepTargets;
    int i = 0;

    public string GetRequest()
    {
        return request;
    }
    public GameObject[] GetSteps()
    {
        return;
    }


}
