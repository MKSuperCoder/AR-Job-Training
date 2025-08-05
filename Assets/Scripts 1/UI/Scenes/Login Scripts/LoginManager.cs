using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class LoginManager : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passwordInput;

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private void Start()
    {
        StartCoroutine(InitializeAndWaitForFirebase());
    }

    private IEnumerator InitializeAndWaitForFirebase()
    {
        FirebaseManager.Instance.InitializeFirebase();

        // Wait until FirebaseAuth and Firestore are available
        yield return new WaitUntil(() => FirebaseManager.Instance.Auth != null && FirebaseManager.Instance.Firestore != null);

        auth = FirebaseManager.Instance.Auth;
        db = FirebaseManager.Instance.Firestore;
    }


    public void OnLoginClick()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Email and password required.");
            return;
        }

        StartCoroutine(LoginUser(email, password));
    }

    private IEnumerator LoginUser(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.IsFaulted || loginTask.IsCanceled)
        {
            Debug.LogError("Login failed.");
            if (loginTask.Exception != null)
            {
                foreach (var e in loginTask.Exception.Flatten().InnerExceptions)
                {
                    Debug.LogError("Login error: " + e.Message);
                }
            }
            yield break;
        }
        Debug.Log("loginTask.IsCompleted: " + loginTask.IsCompleted);
        Debug.Log("loginTask.IsFaulted: " + loginTask.IsFaulted);
        Debug.Log("loginTask.Result: " + (loginTask.Result != null));

        var result = loginTask.Result;
        if (result == null || result.User == null)
        {
            Debug.LogError("Login completed but user is null.");
            yield break;
        }

        var user = result.User;
        string userId = user.UserId;
        Debug.Log("User logged in: " + userId);

        var userDocTask = db.Collection("users").Document(userId).GetSnapshotAsync();
        yield return new WaitUntil(() => userDocTask.IsCompleted);

        if (userDocTask.IsFaulted || userDocTask.IsCanceled)
        {
            Debug.LogError("Failed to fetch user document.");
            yield break;
        }

        var snapshot = userDocTask.Result;
        if (!snapshot.Exists)
        {
            Debug.LogError("User document not found.");
            yield break;
        }

        var data = snapshot.ToDictionary();
        string role = data.ContainsKey("role") ? data["role"].ToString() : "trainee";
        string username = data.ContainsKey("username") ? data["username"].ToString() : "User";

        if (UserSession.Instance == null)
        {
            Debug.LogError("UserSession.Instance is null. Make sure it exists in the scene.");
            yield break;
        }

        UserSession.Instance.StartSession(userId, role, username);
        yield return new WaitForSeconds(1f); // wait briefly to let session data persist
        SceneManager.LoadScene("Home");
    }


}