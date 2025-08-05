using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class UserSession : MonoBehaviour
{
    public static UserSession Instance;

    public string UserId { get; private set; }
    public string Role { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string Age { get; private set; }
    public string SelectedTraineeId { get; private set; }

    private FirebaseFirestore db;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("UserSession Awake: Persisting between scenes.");
    }

    public void StartSession(string userId, string role, string username)
    {
        UserId = userId;
        Role = role;
        Username = username;

        PlayerPrefs.SetString("userId", userId);
        PlayerPrefs.SetString("role", role);
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.Save();
        Debug.Log($"Session started: {userId}, {role}, {username}");
        FetchUserData(userId);
    }


    private void FetchUserData(string userId)
    {
        db.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                var data = task.Result.ToDictionary();
                Name = data.ContainsKey("name") ? data["name"].ToString() : "";
                Email = data.ContainsKey("email") ? data["email"].ToString() : "";
                Username = data.ContainsKey("username") ? data["username"].ToString() : "";
                Age = data.ContainsKey("age") ? data["age"].ToString() : "";

                Debug.Log("User data loaded.");
            }
        });
    }
    public void LoadFromPrefs()
    {
        UserId = PlayerPrefs.GetString("userId", "");
        Role = PlayerPrefs.GetString("role", "");
    }
    public void SetSelectedTrainee(string traineeId)
    {
        SelectedTraineeId = traineeId;

        // Save to PlayerPrefs if you want persistence
        PlayerPrefs.SetString("selectedTraineeId", traineeId);
        PlayerPrefs.Save();
    }

    public void LoadSelectedTrainee()
    {
        SelectedTraineeId = PlayerPrefs.GetString("selectedTraineeId", "");
    }
    public void ResetSession()
    {
        UserId = null;
        Role = null;
        Name = null;
        Email = null;
        Username = null;
        Age = null;
        SelectedTraineeId = null;

        Debug.Log("UserSession has been reset.");
    }

}
