using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Linq;

public class AssignmentManager : MonoBehaviour
{
    [Header("Assign UI")]
    public TMP_Dropdown traineeDropdown;
    public Button assignButton;
    public TMP_Text assignMessageText;

    [Header("Log Viewer UI")]
    public TMP_Dropdown assignedTraineeDropdown;
    public Button loadLogsButton;
    public TMP_Text logsText;

    private FirebaseFirestore db;
    private Dictionary<string, string> usernameToId = new Dictionary<string, string>();

    private string trainerId => UserSession.Instance.UserId;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;
        PopulateAllTrainees();
        PopulateAssignedTrainees();
    }


    void PopulateAllTrainees()
    {
        db.Collection("users")
          .WhereEqualTo("role", "trainee")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted || task.IsCanceled)
              {
                  Debug.LogError("Failed to fetch trainees.");
                  return;
              }

              QuerySnapshot snapshot = task.Result;
              traineeDropdown.ClearOptions();
              usernameToId.Clear();

              List<string> options = new List<string>();

              foreach (DocumentSnapshot doc in snapshot.Documents)
              {
                  if (doc.ContainsField("assignedTrainerID") && !string.IsNullOrEmpty(doc.GetValue<string>("assignedTrainerID")))
                      continue;

                  string username = doc.ContainsField("username") ? doc.GetValue<string>("username") : "(no name)";
                  options.Add(username);
                  usernameToId[username] = doc.Id;
              }

              if (options.Count == 0)
              {
                  options.Add("No unassigned trainees available");
                  assignButton.interactable = false;
              }
              else
              {
                  assignButton.interactable = true;
              }

              traineeDropdown.AddOptions(options);
          });
    }






    // Load currently assigned trainees for log viewer
    void PopulateAssignedTrainees()
    {
        db.Collection("users").Document(trainerId)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (!task.Result.Exists)
              {
                  Debug.LogWarning("Trainer document not found.");
                  return;
              }

              List<string> assigned = new List<string>();

              if (task.Result.ContainsField("assignedTrainees"))
              {
                  assigned = task.Result.GetValue<List<string>>("assignedTrainees");
              }

              assignedTraineeDropdown.ClearOptions();

              if (assigned.Count == 0)
              {
                  assignedTraineeDropdown.AddOptions(new List<string> { "No assigned trainees" });
                  loadLogsButton.interactable = false;
              }
              else
              {
                  assignedTraineeDropdown.AddOptions(assigned);
                  loadLogsButton.interactable = true;
              }
          });
    }


    public void OnAssignClick()
    {
        string selectedUsername = traineeDropdown.options[traineeDropdown.value].text;

        if (!usernameToId.ContainsKey(selectedUsername))
        {
            Debug.LogError("Selected username not found in ID map.");
            assignMessageText.text = " Could not find user ID.";
            return;
        }

        string traineeId = usernameToId[selectedUsername];
        StartCoroutine(Assign(trainerId, traineeId));
    }


    private IEnumerator Assign(string trainerId, string traineeId)
    {
        DocumentReference trainerRef = db.Collection("users").Document(trainerId);
        DocumentReference traineeRef = db.Collection("users").Document(traineeId);

        var batch = db.StartBatch();
        batch.Update(trainerRef, new Dictionary<string, object>
        {
            { "assignedTrainees", FieldValue.ArrayUnion(traineeId) }
        });
        batch.Update(traineeRef, new Dictionary<string, object>
        {
            { "assignedTrainerId", trainerId }
        });

        var batchTask = batch.CommitAsync();
        yield return new WaitUntil(() => batchTask.IsCompleted);

        if (batchTask.IsFaulted || batchTask.IsCanceled)
            assignMessageText.text = "Failed to assign.";
        else
        {
            assignMessageText.text = $"Assigned {usernameToId.FirstOrDefault(kv => kv.Value == traineeId).Key}.";;
            PopulateAllTrainees();    // Refresh unassigned list
            PopulateAssignedTrainees(); // Refresh trainer’s assigned list
        }
    }


    public void OnLoadLogsClick()
    {
        string selectedTraineeId = assignedTraineeDropdown.options[assignedTraineeDropdown.value].text;
        LoadLogs(selectedTraineeId);
    }

    void LoadLogs(string traineeId)
    {
        db.Collection("traineeLogs")
          .WhereEqualTo("traineeId", traineeId)
          .OrderBy("timestamp")
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsFaulted) return;

              logsText.text = $"Logs for {traineeId}:\n\n";
              foreach (var doc in task.Result.Documents)
              {
                  string msg = doc.ContainsField("message") ? doc.GetValue<string>("message") : "(no message)";
                  logsText.text += $"- {msg}\n";
              }
          });
    }
}
