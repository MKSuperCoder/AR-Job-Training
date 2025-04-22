using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class SignupManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] TMP_InputField ageInput;
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passwordInput;
    

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
                Debug.LogError("Firebase init error: " + task.Result);
                
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

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(age) ||
            string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            
            return;
        }

        StartCoroutine(SignUpUser(name, age, username, email, password));
    }

    private IEnumerator SignUpUser(string name, string age, string username, string email, string password)
    {
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.IsFaulted || registerTask.IsCanceled)
        {
            foreach (var e in registerTask.Exception.Flatten().InnerExceptions)
            {
                Debug.LogError("Sign-up error: " + e.Message);
            }
            yield break;
        }

        Firebase.Auth.AuthResult result = registerTask.Result;
        FirebaseUser newUser = result.User;
        string userId = newUser.UserId;

        var userDoc = db.Collection("users").Document(userId);
        var userData = new Dictionary<string, object>
        {
            { "name", name },
            { "age", age },
            { "username", username },
            { "email", email },
            { "password", password },
            { "createdAt", Timestamp.GetCurrentTimestamp() }
        };

        var saveTask = userDoc.SetAsync(userData);
        yield return new WaitUntil(() => saveTask.IsCompleted);

        if (saveTask.IsFaulted || saveTask.IsCanceled)
        {
            Debug.LogError("Error saving user data: " + saveTask.Exception);
            
        }
        else
        {
            
            SceneManager.LoadScene("MainMenu");
        }
        SceneManager.LoadScene("MainMenu");
    }
}
