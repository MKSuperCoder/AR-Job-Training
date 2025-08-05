using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

public class ARClickHandler : MonoBehaviour
{
    public enum ButtonType { Strength, Fresh, Clean, Brew, TimeIncrease, TimeDecrease }
    public ButtonType buttonType;
    private Vector3 originalCupPosition;
    private Camera arCamera;
    public TMP_Text strengthText;
    public TMP_Text typeText;
    public TMP_Text coffeeTimeText;
    public GameObject brewedCoffee;      // Shows for 2 seconds
    public GameObject servedCoffee;      // Shown after 2 seconds
    public GameObject cup;
    private int strengthLevel = 0;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (EventSystem.current.IsPointerOverGameObject(0)) return; // Ignore UI touches

            Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
            int interactableLayerMask = LayerMask.GetMask("Interactable");
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, interactableLayerMask))
            { 
                if (hit.transform.CompareTag("ARButton"))
                {
                    Debug.Log("AR 3D button clicked!");
                    TriggerYourFunction(hit.transform.gameObject);
                }
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log("3D object clicked!");
        TriggerYourFunction(gameObject);
    }

    void TriggerYourFunction(GameObject clickedObject)
    {
        Renderer rend = clickedObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.green;
        }

        StartCoroutine(ResetColor(clickedObject, 0.3f));
        ARClickHandler handler = clickedObject.GetComponent<ARClickHandler>();
        if (handler == null) return;

        CoffeeMachineTrainingManager trainingManager = FindObjectOfType<CoffeeMachineTrainingManager>();

        if (buttonType != ButtonType.Brew)
        {
            if (trainingManager != null)
            {
                bool valid = trainingManager.RegisterButtonClick(buttonType);
                if (!valid) return;
            }
        }

        switch (handler.buttonType)
        {
            case ButtonType.Strength:
                strengthLevel++;
                strengthLevel = Mathf.Clamp(strengthLevel, 0, 3); 

                string strengthValue = strengthLevel switch
                {
                    0 => "None",
                    1 => "Low",
                    2 => "Medium",
                    3 => "High",
                    _ => "None"
                };

                if (strengthText != null)
                    strengthText.text = $"Strength: {strengthValue}";

                CoffeeMachineTrainingManager coffeeManager = FindObjectOfType<CoffeeMachineTrainingManager>();
                if (coffeeManager != null)
                    coffeeManager.OnStrengthSelected(strengthValue);
                break;


            case ButtonType.Fresh:
                if (typeText != null)
                    typeText.text = "Type: Fresh";
                break;

            case ButtonType.Clean:
                if (typeText != null)
                    typeText.text = "Type: Clean";
                trainingManager.OnTaskCompleted();
                trainingManager.ShowCleaningTextThenReport();
                break;

            case ButtonType.TimeIncrease:
                trainingManager?.OnTimeIncrease();
                break;

            case ButtonType.TimeDecrease:
                trainingManager?.OnTimeDecrease();
                break;

            case ButtonType.Brew:
                if (trainingManager.RegisterButtonClick(this.buttonType))
                {
                    trainingManager?.NotifyTrainer("BrewStarted");
                    StartCoroutine(HandleBrewSequence());
                }
                break;
        }

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        string sessionDocId = UserSession.Instance.UserId + "_session";

        Dictionary<string, object> update = new Dictionary<string, object>
        {
            { "lastButton", buttonType.ToString() },
            { "lastClickedAt", Timestamp.GetCurrentTimestamp() },
            { "strengthLevel", strengthLevel },
            { "typeText", typeText != null ? typeText.text : "" }
        };

        db.Collection("liveSessions").Document(sessionDocId).SetAsync(update, SetOptions.MergeAll);
    }

    private IEnumerator HandleBrewSequence()
    {
        if (cup != null)
            originalCupPosition = cup.transform.position;

        if (brewedCoffee != null)
            brewedCoffee.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (brewedCoffee != null)
            brewedCoffee.SetActive(false);

        if (servedCoffee != null)
        {
            servedCoffee.SetActive(true);
            yield return new WaitForSeconds(2f);
            StartCoroutine(MoveCup(cup, 0.869f));

            yield return new WaitForSeconds(1.5f);
            ResetCup();

            CoffeeMachineTrainingManager trainingManager = FindObjectOfType<CoffeeMachineTrainingManager>();
            if (trainingManager != null)
            {
                trainingManager.OnTaskCompleted();
                ARClickHandler handler = FindObjectOfType<ARClickHandler>();
                if (handler != null)
                {
                    handler.ResetStrengthLevel();
                }
            }
        }
    }

    private IEnumerator MoveCup(GameObject cup, float targetX)
    {
        Vector3 start = cup.transform.position;
        Vector3 end = start + new Vector3(0.8f, 0, 0);
        float duration = 1f;
        float t = 0f;

        while (t < duration)
        {
            cup.transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        cup.transform.position = end;

        Rigidbody rb = cup.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    IEnumerator ResetColor(GameObject clickedObject, float delay)
    {
        yield return new WaitForSeconds(delay);

        Renderer rend = clickedObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.white;
        }
    }

    private void ResetCup()
    {
        if (cup != null)
        {
            cup.transform.position = originalCupPosition;

            Rigidbody rb = cup.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        if (servedCoffee != null)
            servedCoffee.SetActive(false);
    }
    public void ResetStrengthLevel()
    {
        strengthLevel = 0;
        if (strengthText != null)
            strengthText.text = "Strength: None";
    }
}
