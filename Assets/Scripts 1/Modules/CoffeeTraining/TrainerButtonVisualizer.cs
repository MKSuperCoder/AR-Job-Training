using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections.Generic;

public class TrainerButtonVisualizer : MonoBehaviour
{
    public ARClickHandler[] arButtons; // Assign all ARClickHandler-based buttons in Inspector

    private FirebaseFirestore db;
    private ListenerRegistration listener;

    private string traineeId => UserSession.Instance.SelectedTraineeId;
    private string sessionDocId => traineeId + "_session";

    void Start()
    {
        db = FirebaseManager.Instance.Firestore; // Use the centralized FirebaseManager

        listener = db.Collection("liveSessions").Document(sessionDocId)
            .Listen(snapshot =>
            {
                if (snapshot.Exists && snapshot.ContainsField("lastButton"))
                {
                    string lastButton = snapshot.GetValue<string>("lastButton");
                    HighlightButton(lastButton);
                }
            });
    }


    void HighlightButton(string buttonName)
    {
        ResetColors();
        foreach (var btn in arButtons)
        {
            Renderer rend = btn.GetComponent<Renderer>();
            if (rend != null)
            {
                // Reset all button colors to white
                rend.material.color = Color.white;

                // Highlight the correct one
                if (btn.buttonType.ToString() == buttonName)
                {
                    rend.material.color = Color.green;
                }
            }
        }
    }
    void ResetColors()
    {
        foreach (var btn in arButtons)
        {
            var rend = btn.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = Color.white;
        }
    }

    void OnDestroy()
    {
        listener?.Stop();
    }
}
