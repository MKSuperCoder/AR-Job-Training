using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;

public class ProfileManager : MonoBehaviour
{
    [SerializeField] TMP_Text name;
    [SerializeField] TMP_Text age;
    [SerializeField] TMP_Text taskCompletionRate;
    [SerializeField] TMP_Text taskAccuracy;
    [SerializeField] TMP_Text level;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    // Start is called before the first frame update
    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            LoadUserName(auth.CurrentUser.UserId);
        }
        
    }

    private void LoadUserName(string userId)
    {
        firestore.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully && task.Result.Exists)
            {
                string username = task.Result.GetValue<string>("name");
                name.text = $"Name: {username}";
                string userAge = task.Result.GetValue<string>("age");
                age.text = $"Age: {userAge}";
            }
            
        });
    }

    public void Logout()
    {
        auth.SignOut();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
    }
}
