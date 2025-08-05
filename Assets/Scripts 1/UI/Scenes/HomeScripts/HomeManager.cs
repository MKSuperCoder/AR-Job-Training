using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
 
    [Header("Trainee UI")]
    [SerializeField] GameObject traineePanel;
    [SerializeField] TMP_Text traineeWelcomeText;
    [SerializeField] Button informationButton;
    public GameObject information;
    public GameObject[] informationText;

    [Header("Trainer UI")]
    [SerializeField] GameObject trainerPanel;
    [SerializeField] TMP_Text trainerWelcomeText;

    private FirebaseAuth auth;


    void Awake()
    {
        Debug.Log("HomeManager Awake called.");
    }

    void Start()
    {
        Debug.Log("Start method has been called");
        auth = FirebaseManager.Instance.Auth;
    
        Debug.Log($"UserId: {UserSession.Instance?.UserId}");
        Debug.Log($"Role: {UserSession.Instance?.Role}");
        Debug.Log($"Username: {UserSession.Instance?.Username}");


        // Disable both panels initially
        Debug.Log("Both panels deactivated");
        traineePanel.SetActive(false);
        trainerPanel.SetActive(false);

        if (UserSession.Instance != null && !string.IsNullOrEmpty(UserSession.Instance.UserId))
        {
            string username = UserSession.Instance.Username;
            string role = UserSession.Instance.Role;

            if (role == "trainer")
            {
                trainerPanel.SetActive(true);
                Debug.Log("Trainer Panel Activated");
                trainerWelcomeText.text = $"Welcome, Trainer {username}. You are logged in as a trainer.";
            }
            else
            {
                Debug.Log("Trainee Panel Activated");
                traineePanel.SetActive(true);
                traineeWelcomeText.text = $"Welcome, Trainee {username}. You are logged in as a trainee.";
            }
        }
        else
        {
     
            Debug.LogWarning("UserSession data is not loaded.");
        }
    }


    public void Logout()
    {
        auth.SignOut();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }

    public void displayInformation(int index)
    {
        information.SetActive(true);
        informationText[index].SetActive(true);
    }
    public void hideInformation(int index)
    {
        informationText[index].SetActive(false);
    }
}
