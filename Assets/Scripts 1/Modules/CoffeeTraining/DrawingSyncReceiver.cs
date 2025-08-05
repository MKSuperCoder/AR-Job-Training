using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Firestore;
using TMPro;
using System.Collections;

public class DrawingSyncReceiver : MonoBehaviour
{
    public RawImage drawingImage;
    public TMP_Text trainerStrengthText;
    public TMP_Text trainerTypeText;

    private FirebaseFirestore db => FirebaseManager.Instance.Firestore;
    private ListenerRegistration drawingListener;
    private Texture2D drawingTexture;

    private int texWidth = 1080;
    private int texHeight = 1920;
    private string sessionDocId => UserSession.Instance.UserId + "_session";

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        if (drawingImage == null)
        {
            Debug.LogError("DrawingSyncReceiver: RawImage not assigned.");
            yield break;
        }

        InitializeTexture();
        ListenToDrawing();
    }

    void InitializeTexture()
    {
        texWidth = Mathf.Max(1, (int)drawingImage.rectTransform.rect.width);
        texHeight = Mathf.Max(1, (int)drawingImage.rectTransform.rect.height);
        drawingTexture = new Texture2D(texWidth, texHeight, TextureFormat.RGBA32, false);

        drawingTexture.filterMode = FilterMode.Bilinear;
        drawingTexture.wrapMode = TextureWrapMode.Clamp;

        ClearTexture();

        drawingImage.texture = drawingTexture;
        drawingImage.color = new Color(1f, 1f, 1f, 1f);
        drawingImage.raycastTarget = false;
    }

    void ClearTexture()
    {
        Color32[] clearPixels = new Color32[texWidth * texHeight];
        for (int i = 0; i < clearPixels.Length; i++)
            clearPixels[i] = new Color32(0, 0, 0, 0); // Transparent

        drawingTexture.SetPixels32(clearPixels);
        drawingTexture.Apply();
    }

    void ListenToDrawing()
    {
        if (string.IsNullOrEmpty(sessionDocId))
        {
            Debug.LogError("[DrawingSyncReceiver] sessionDocId is null or empty.");
            return;
        }

        Debug.Log("[DrawingSyncReceiver] Listening to doc: " + sessionDocId);

        drawingListener = db.Collection("liveSessions").Document(sessionDocId)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists) return;

                if (snapshot.ContainsField("drawingOverlay"))
                {
                    var points = snapshot.GetValue<List<object>>("drawingOverlay");
                    UpdateDrawing(points);
                }
            });
    }

    void UpdateDrawing(List<object> points)
    {
        ClearTexture();
        if (points == null || points.Count < 2) return;

        Vector2? prevPixel = null;

        foreach (var obj in points)
        {
            if (obj is Dictionary<string, object> dict &&
                dict.TryGetValue("x", out object xObj) &&
                dict.TryGetValue("y", out object yObj) &&
                float.TryParse(xObj.ToString(), out float normX) &&
                float.TryParse(yObj.ToString(), out float normY))
            {
                if (normX < 0f || normX > 1f || normY < 0f || normY > 1f)
                {
                    Debug.LogWarning("Drawing point out of bounds. Skipped.");
                    continue;
                }

                int px = Mathf.RoundToInt(normX * texWidth);
                int py = Mathf.RoundToInt(normY * texHeight);

                if (prevPixel.HasValue)
                {
                    DrawLine((int)prevPixel.Value.x, (int)prevPixel.Value.y, px, py, new Color(0f, 1f, 0f, 0.8f)); // green line, semi-transparent
                }

                prevPixel = new Vector2(px, py);
            }
        }

        drawingTexture.Apply();
    }

    void DrawLine(int x0, int y0, int x1, int y1, Color color)
    {
        int dx = Mathf.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
        int err = dx + dy, e2;

        while (true)
        {
            if (x0 >= 0 && x0 < texWidth && y0 >= 0 && y0 < texHeight)
                drawingTexture.SetPixel(x0, y0, color);

            if (x0 == x1 && y0 == y1) break;
            e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    void OnDestroy()
    {
        drawingListener?.Stop();
    }
}
