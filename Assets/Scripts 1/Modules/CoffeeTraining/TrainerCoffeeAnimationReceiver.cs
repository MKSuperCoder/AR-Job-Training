using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using System.Collections;

public class TrainerCoffeeAnimationReceiver : MonoBehaviour
{
    public GameObject brewedCoffee;
    public GameObject servedCoffee;
    public GameObject cup;

    private Vector3 originalCupPos;
    private ListenerRegistration listener;

    void Start()
    {
        if (cup != null)
            originalCupPos = cup.transform.position;

        if (UserSession.Instance == null || string.IsNullOrEmpty(UserSession.Instance.SelectedTraineeId))
        {
            Debug.LogWarning("No selected trainee ID. Cannot listen to animation events.");
            return;
        }

        string sessionDocId = UserSession.Instance.SelectedTraineeId + "_session";

        listener = FirebaseManager.Instance.Firestore
            .Collection("liveSessions")
            .Document(sessionDocId)
            .Listen(snapshot =>
            {
                if (!snapshot.Exists)
                    return;

                if (snapshot.TryGetValue("animationStep", out string step))
                {
                    Debug.Log("Received animation step: " + step);
                    TriggerAnimation(step);
                }
            });
    }

    void TriggerAnimation(string step)
    {
        switch (step)
        {
            case "BrewStarted":
                StartCoroutine(PlayBrewAnimation());
                break;
        }
    }

    private IEnumerator PlayBrewAnimation()
    {
        if (brewedCoffee != null)
            brewedCoffee.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (brewedCoffee != null)
            brewedCoffee.SetActive(false);

        if (servedCoffee != null)
        {
            servedCoffee.SetActive(true);
            yield return new WaitForSeconds(2f);
            yield return MoveCup(cup, 0.869f);
        }
    }

    private IEnumerator MoveCup(GameObject cupObj, float targetX)
    {
        if (cupObj == null) yield break;

        Vector3 start = cupObj.transform.position;
        Vector3 end = start + new Vector3(targetX, 0, 0);
        float duration = 1f;
        float t = 0f;

        while (t < duration)
        {
            cupObj.transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        cupObj.transform.position = end;

        Rigidbody rb = cupObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        yield return new WaitForSeconds(1.5f);
        ResetCup();
    }

    private void ResetCup()
    {
        if (cup == null)
            return;

        cup.transform.position = originalCupPos;

        Rigidbody rb = cup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (servedCoffee != null)
            servedCoffee.SetActive(false);
    }

    void OnDestroy()
    {
        listener?.Stop();
    }
}
