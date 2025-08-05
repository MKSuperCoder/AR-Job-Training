using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;

public class TrainerDrawingUploader : MonoBehaviour
{
    private FirebaseFirestore db;
    private string traineeId => UserSession.Instance.SelectedTraineeId;
    private string sessionDocId => traineeId + "_session";

    private List<Vector2> drawingBuffer = new List<Vector2>();
    private float uploadInterval = 0.2f;
    private float timer = 0f;
    public UnityEngine.UI.RawImage drawingCanvas;

    void Start()
    {
        db = FirebaseManager.Instance.Firestore;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= uploadInterval && drawingBuffer.Count > 0)
        {
            UploadDrawingData();
            drawingBuffer.Clear();
            timer = 0f;
        }
    }

    public void AddDrawingPoint(Vector2 screenPoint)
    {
        if (drawingCanvas == null)
        {
            Debug.LogError("TrainerDrawingUploader: drawingCanvas is not assigned!");
            return;
        }

        RectTransform rect = drawingCanvas.rectTransform;

        if (rect == null)
        {
            Debug.LogError("drawingCanvas does not have a valid RectTransform.");
            return;
        }

        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPoint, null, out localPoint))
        {
            Debug.LogWarning("ScreenPoint could not be converted to local point.");
            return;
        }

        Vector2 size = rect.sizeDelta;
        Vector2 normalized = new Vector2(localPoint.x / size.x + 0.5f, localPoint.y / size.y + 0.5f);

        drawingBuffer.Add(normalized);
    }

    void UploadDrawingData()
    {
        // Limit to 2 points for debugging
        var limitedBuffer = drawingBuffer.Count > 2
            ? drawingBuffer.GetRange(0, 2)
            : new List<Vector2>(drawingBuffer);

        var serializedPoints = new List<Dictionary<string, object>>();
        foreach (var point in limitedBuffer)
        {
            serializedPoints.Add(new Dictionary<string, object>
            {
                { "x", point.x },
                { "y", point.y }
            });
        }

        var drawingData = new Dictionary<string, object>
        {
            { "drawingOverlay", serializedPoints },
            { "drawingTimestamp", Timestamp.GetCurrentTimestamp() }
        };

        db.Collection("liveSessions")
          .Document(sessionDocId)
          .SetAsync(drawingData, SetOptions.MergeAll)
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompletedSuccessfully)
              {
                  Debug.Log($"[Trainer] Uploaded {serializedPoints.Count} points to: {sessionDocId}");
              }
              else
              {
                  Debug.LogError("Drawing upload failed: " + task.Exception);
              }
          });
    }
}
