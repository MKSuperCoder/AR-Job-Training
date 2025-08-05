using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.UI;
using System.Collections.Generic;

public class DrawingTextureReceiver : MonoBehaviour
{
    public RawImage drawingImage;
    private Texture2D drawingTexture;

    private FirebaseFirestore db;
    private string traineeId => UserSession.Instance.SelectedTraineeId;
    private string sessionDocId => traineeId + "_session";
    private ListenerRegistration listener;

    private int texWidth = 1920;
    private int texHeight = 1080;

    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        drawingTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);
        ClearTexture();
        drawingImage.texture = drawingTexture;

        ListenToDrawing();
    }

    void ClearTexture()
    {
        Color[] clearPixels = new Color[texWidth * texHeight];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = Color.clear;

        drawingTexture.SetPixels(clearPixels);
        drawingTexture.Apply();
    }

    void ListenToDrawing()
    {
        listener = db.Collection("liveSessions").Document(sessionDocId).Listen(snapshot =>
        {
            if (!snapshot.Exists || !snapshot.ContainsField("drawingOverlay")) return;

            var points = snapshot.GetValue<List<object>>("drawingOverlay");
            DrawPoints(points);
        });
    }

    void DrawPoints(List<object> points)
    {
        foreach (var obj in points)
        {
            if (obj is Dictionary<string, object> dict &&
                dict.TryGetValue("x", out object xObj) &&
                dict.TryGetValue("y", out object yObj))
            {
                if (float.TryParse(xObj.ToString(), out float x) &&
                    float.TryParse(yObj.ToString(), out float y))
                {
                    int px = Mathf.Clamp((int)x, 0, texWidth - 1);
                    int py = Mathf.Clamp((int)y, 0, texHeight - 1);
                    drawingTexture.SetPixel(px, py, Color.red); // Change color here if needed
                }
            }
        }
        drawingTexture.Apply();
    }

    void OnDestroy()
    {
        listener?.Stop();
    }
}
