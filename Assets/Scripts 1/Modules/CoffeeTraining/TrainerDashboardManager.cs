using UnityEngine;
using TMPro;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TrainerDashboardManager : MonoBehaviour
{
    public TMP_Text traineeIdText;
    public TMP_Text moduleText;
    public TMP_Text stepText;
    public TMP_Text hintText;
    public TMP_Text requestText;         // ✅ Shows the customer request
    public TMP_Text instructionsText;    // ✅ Shows the instruction steps

    public RawImage drawingCanvas;
    public Button resetStepButton;
    public TMP_InputField hintInputField;
    public Button sendHintButton;

    private FirebaseFirestore db;
    private ListenerRegistration sessionListener;

    private string traineeId => UserSession.Instance.SelectedTraineeId;
    private string sessionDocId => traineeId + "_session";

    private TrainerDrawingUploader drawingUploader;

    void Start()
    {
        db = FirebaseManager.Instance.Firestore;
        drawingUploader = GetComponent<TrainerDrawingUploader>();

        if (string.IsNullOrEmpty(traineeId))
        {
            Debug.LogError("SelectedTraineeId is null or empty. Cannot listen to live session.");
            return;
        }

        Debug.Log("Listening to live session for traineeId: " + traineeId);
        ListenToLiveSession();

        resetStepButton.onClick.AddListener(ResetStep);
        sendHintButton.onClick.AddListener(SendHint);
    }

    void ListenToLiveSession()
    {
        sessionListener = db.Collection("liveSessions").Document(sessionDocId)
            .Listen(async snapshot =>
            {
                Debug.Log("Live session snapshot received.");

                if (!snapshot.Exists) return;

                Dictionary<string, object> data = snapshot.ToDictionary();

                string module = data.ContainsKey("module") ? data["module"].ToString() : "(none)";
                string hint = data.ContainsKey("hint") ? data["hint"].ToString() : "(none)";
                long step = data.ContainsKey("activeStep") ? (long)data["activeStep"] : -1;

                string traineeUsername = await FetchUsernameAsync(traineeId);

                traineeIdText.text = $"Trainee: {traineeUsername} ({traineeId})";
                moduleText.text = $"Module: {module}";
                stepText.text = $"Step: {step}";
                hintText.text = $"Hint: {hint}";

                // ✅ New: Show Request and Instructions
                if (data.ContainsKey("requestText"))
                    requestText.text = data["requestText"].ToString();

                if (data.ContainsKey("instructions"))
                    instructionsText.text = data["instructions"].ToString();
            });
    }

    async Task<string> FetchUsernameAsync(string uid)
    {
        var userDoc = await db.Collection("users").Document(uid).GetSnapshotAsync();
        if (userDoc.Exists && userDoc.ContainsField("username"))
            return userDoc.GetValue<string>("username");
        return uid;
    }

    void ResetStep()
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "resetStep", true },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        db.Collection("liveSessions").Document(sessionDocId).UpdateAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                    Debug.Log("Step reset sent.");
                else
                    Debug.LogError("Failed to reset step.");
            });
    }

    public void SendHint()
    {
        string customHint = hintInputField.text.Trim();
        if (string.IsNullOrEmpty(customHint)) return;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "hint", customHint },
            { "timestamp", Timestamp.GetCurrentTimestamp() }
        };

        db.Collection("liveSessions").Document(sessionDocId).UpdateAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("Hint sent.");
                    hintInputField.text = "";
                }
                else
                    Debug.LogError("Failed to send hint.");
            });
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (drawingCanvas == null || drawingUploader == null) return;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                drawingCanvas.rectTransform,
                Input.mousePosition,
                null,
                out Vector2 localPos
            );

            drawingUploader.AddDrawingPoint(localPos);
        }
    }

    void OnDestroy()
    {
        sessionListener?.Stop();
        Debug.Log("Stopped listening to live session.");
    }
}
