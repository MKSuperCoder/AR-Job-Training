using UnityEngine;

public class CoffeeMachineSceneManager : MonoBehaviour
{
    public GameObject traineeUI;
    public GameObject trainerUI;

    public GameObject traineeScriptObject; // has CoffeeMachineTrainingManager, DrawingSyncReceiver
    public GameObject trainerScriptObject; // has TrainerDashboardManager, DrawingUploader

    void Start()
    {
        string role = UserSession.Instance.Role;

        if (role == "trainee")
        {
            traineeUI.SetActive(true);
            trainerUI.SetActive(false);

            traineeScriptObject.SetActive(true);
            trainerScriptObject.SetActive(false);
        }
        else if (role == "trainer")
        {
            traineeUI.SetActive(false);
            trainerUI.SetActive(true);

            traineeScriptObject.SetActive(false);
            trainerScriptObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Unknown user role.");
        }
    }
}
