using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;

public class LiveSessionListManager : MonoBehaviour
{
    public Transform sessionListContent; // Content panel of ScrollView
    public GameObject sessionEntryPrefab; // Prefab with text + join button

    private FirebaseFirestore db;

    void Start()
    {
        db = FirebaseManager.Instance.Firestore;
        LoadActiveSessions();
    }

    void LoadActiveSessions()
    {
        db.Collection("liveSessions").Listen(async snapshot =>
        {
            if (sessionListContent != null)
            {
                for (int i = sessionListContent.childCount - 1; i >= 0; i--)
                {
                    Destroy(sessionListContent.GetChild(i).gameObject);
                }
            }

            foreach (var doc in snapshot.Documents)
            {
                if (!doc.Exists || !doc.ContainsField("traineeId") || !doc.ContainsField("module"))
                {
                    Debug.LogWarning($"Skipping malformed session document: {doc.Id}");
                    continue;
                }

                string traineeId = doc.GetValue<string>("traineeId");
                string module = doc.GetValue<string>("module");

                string traineeUsername = await FetchUsernameAsync(traineeId);

                GameObject entry = Instantiate(sessionEntryPrefab, sessionListContent);
                entry.transform.Find("TraineeText").GetComponent<TMP_Text>().text = $"Trainee: {traineeUsername}";
                entry.transform.Find("ModuleText").GetComponent<TMP_Text>().text = $"Module: {module}";

                Button joinBtn = entry.transform.Find("JoinButton").GetComponent<Button>();
                joinBtn.onClick.AddListener(() => JoinLiveSession(traineeId));
            }
        });
    }

    async Task<string> FetchUsernameAsync(string userId)
    {
        var userRef = db.Collection("users").Document(userId);
        var userSnapshot = await userRef.GetSnapshotAsync();

        if (userSnapshot.Exists && userSnapshot.ContainsField("username"))
            return userSnapshot.GetValue<string>("username");

        return "(unknown)";
    }

    void JoinLiveSession(string traineeId)
    {
        string sessionDocId = traineeId + "_session";
        string trainerId = FirebaseManager.Instance.Auth.CurrentUser.UserId;

        FirebaseFirestore db = FirebaseManager.Instance.Firestore;
        DocumentReference sessionRef = db.Collection("liveSessions").Document(sessionDocId);

        // Save trainee ID for the trainer session
        UserSession.Instance.SetSelectedTrainee(traineeId); // ✅ Set SelectedTraineeId for dashboard

        // Assign trainerId in the session doc
        sessionRef.UpdateAsync(new Dictionary<string, object>
    {
        { "trainerId", trainerId },
        { "timestamp", Timestamp.GetCurrentTimestamp() }
    }).ContinueWithOnMainThread(task =>
    {
        if (task.IsCompletedSuccessfully)
        {
            PlayerPrefs.SetString("targetTraineeId", traineeId); // Optional backup
            UnityEngine.SceneManagement.SceneManager.LoadScene("CoffeeTraining");
        }
        else
        {
            Debug.LogError("Failed to assign trainer to session: " + task.Exception);
        }
    });
    }

}
