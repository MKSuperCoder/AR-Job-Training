using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class SignupManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField ageInput;
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Dropdown roleDropdown; // e.g., "Trainee" or "Trainer"

    private FirebaseAuth auth;
    private FirebaseFirestore db;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
            }
            else
            {
                Debug.LogError("Firebase not available: " + task.Result);
            }
        });
    }

    public void OnSignUpClick()
    {
        string name = nameInput.text.Trim();
        string age = ageInput.text.Trim();
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string role = roleDropdown.options[roleDropdown.value].text.ToLower(); // "trainee" or "trainer"

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(age) ||
            string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Please fill in all fields.");
            return;
        }

        StartCoroutine(SignUpUser(name, age, username, email, password, role));
    }

    private IEnumerator SignUpUser(string name, string age, string username, string email, string password, string role)
    {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.IsFaulted || registerTask.IsCanceled)
        {
            Debug.LogError("Signup failed: " + registerTask.Exception);
            yield break;
        }

        var newUser = registerTask.Result.User;
        string userId = newUser.UserId;

        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "name", name },
            { "age", age },
            { "username", username },
            { "email", email },
            { "role", role },
            { "createdAt", Timestamp.GetCurrentTimestamp() }
        };

        var userDoc = db.Collection("users").Document(userId).SetAsync(userData);
        yield return new WaitUntil(() => userDoc.IsCompleted);

        if (userDoc.IsFaulted || userDoc.IsCanceled)
        {
            Debug.LogError("Error saving user data to Firestore: " + userDoc.Exception);
            yield break;
        }


        UserSession.Instance.StartSession(userId, role, username);
        Debug.Log("User session started for: " + userId);


        SceneManager.LoadScene("Home");
    }
}
