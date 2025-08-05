using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public void GoToHome()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }

    public void GoToProfile()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("UserProfile");
    }

    public void GoToModuleLibrary()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ModuleLibrary");
    }
    public void GoToDashboard()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("TrainerDashboard");
    }
    public void GoToLiveSessions()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LiveSessionSelection");
    }
    public void GoToTutorial()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
    }
    public void LogOut()
    {
        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.DeleteKey("role");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("selectedTraineeId");
        PlayerPrefs.Save();

        // Reset UserSession values
        if (UserSession.Instance != null)
        {
            UserSession.Instance.ResetSession();
        }

        // Optionally sign out from Firebase Auth
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();

        // Go to login
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }

}
