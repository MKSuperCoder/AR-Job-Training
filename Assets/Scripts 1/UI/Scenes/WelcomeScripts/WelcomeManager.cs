using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WelcomeManager : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f); // Splash screen delay

        if (UserSession.Instance != null)
        {
            UserSession.Instance.LoadFromPrefs();
            string userId = UserSession.Instance.UserId;
            string role = UserSession.Instance.Role;

            if (string.IsNullOrEmpty(userId))
            {
                Debug.Log("No user ID found. Going to Login...");
                SceneManager.LoadScene("Login");
                yield break;
            }

            if (string.IsNullOrEmpty(role))
            {
                Debug.Log("User logged in but role not set. Going to RoleSelect...");
                SceneManager.LoadScene("RoleSelect");
                yield break;
            }

            if (role == Roles.Trainer)
                SceneManager.LoadScene("TrainerDashboard");
            else if (role == Roles.Trainee)
                SceneManager.LoadScene("TraineeModuleSelect");
            else
                SceneManager.LoadScene("RoleSelect");
        }
        else
        {
            Debug.Log("UserSession missing. Going to Login...");
            SceneManager.LoadScene("Login");
        }
    }
}