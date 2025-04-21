using System.Collections;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;

public class LoginManager : MonoBehaviour
{
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] TMP_InputField passwordInput;
    [SerializeField] TMP_Text messageText;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim().ToLower(); // Optional: case-insensitive
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please enter both username and password.";
            return;
        }

        StartCoroutine(LoginUser(username, password));
    }

    private IEnumerator LoginUser(string username, string password)
    {
        var queryTask = firestore.Collection("users")
                                 .WhereEqualTo("username", username)
                                 .GetSnapshotAsync();

        yield return new WaitUntil(() => queryTask.IsCompleted);

        if (queryTask.IsFaulted || queryTask.IsCanceled)
        {
            messageText.text = "Error connecting to database.";
            yield break;
        }

        // Convert Documents to List so we can use [0]
        List<DocumentSnapshot> docs = queryTask.Result.Documents.ToList();

        if (docs == null || docs.Count == 0)
        {
            messageText.text = "Username not found.";
            yield break;
        }

        DocumentSnapshot doc = docs[0];

        if (!doc.ContainsField("email"))
        {
            messageText.text = "User record is missing email.";
            yield break;
        }

        string email = doc.GetValue<string>("email");

        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.IsFaulted || loginTask.IsCanceled)
        {
            foreach (var e in loginTask.Exception.Flatten().InnerExceptions)
            {
                messageText.text = "Login failed: " + e.Message;
                Debug.LogError("Login error: " + e.Message);
            }
        }
        else
        {
            messageText.text = "Login successful!";
            SceneManager.LoadScene("MainMenu");
        }
    }

}
