using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class HomeManager : MonoBehaviour
{
    [SerializeField] TMP_Text welcomeText;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            LoadUserName(auth.CurrentUser.UserId);
        }
        else
        {
            welcomeText.text = "Welcome!";
        }
    }

    private void LoadUserName(string userId)
    {
        firestore.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                string username = task.Result.GetValue<string>("username");
                welcomeText.text = $"Welcome, {username}!";
            }
            else
            {
                welcomeText.text = "Welcome!";
            }
        });
    }

    public void GoToProfile()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("UserProfile");
    }

    public void GoToGameModes()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameModeScene");
    }

    public void Logout()
    {
        auth.SignOut();
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
