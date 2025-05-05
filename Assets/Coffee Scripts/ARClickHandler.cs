using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;


public class ARClickHandler : MonoBehaviour
{
    public enum ButtonType { Strength, Fresh, Clean, Brew }
    public ButtonType buttonType;
    private Vector3 originalCupPosition;
    private Camera arCamera;
    public TMP_Text strengthText;
    public TMP_Text typeText;
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
            if (Physics.Raycast(ray, out RaycastHit hit))
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
        // Change the color of the clicked object
        Renderer rend = clickedObject.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.green; // Set your clicked color here
        }

        // (Optional) Reset color back after delay
        StartCoroutine(ResetColor(clickedObject, 0.3f));
        ARClickHandler handler = clickedObject.GetComponent<ARClickHandler>();
        if (handler == null) return;

        switch (handler.buttonType)
        { 
            case ButtonType.Strength:
                strengthLevel++;
                if (strengthText != null)
                    strengthText.text = $"Strength: {strengthLevel}";
                break;

            case ButtonType.Fresh:
                if (typeText != null)
                    typeText.text = "Type: Fresh";
                break;

            case ButtonType.Clean:
                if (typeText != null)
                    typeText.text = "Type: Clean";
                break;

            case ButtonType.Brew:
                StartCoroutine(HandleBrewSequence());
                break;
        }

    }
    private IEnumerator HandleBrewSequence()
    {
        if (cup != null)
            originalCupPosition = cup.transform.position; // Store initial position

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

            // Reset cup after delay
            yield return new WaitForSeconds(1.5f);
            ResetCup();

            // Notify that task is complete
            CoffeeMachineTrainingManager trainingManager = FindObjectOfType<CoffeeMachineTrainingManager>();
            if (trainingManager != null)
            {
                trainingManager.OnTaskCompleted(); // trigger next task and save result
            }
        }

    }

    private IEnumerator MoveCup(GameObject cup, float targetX)
    {
        Vector3 start = cup.transform.position;
        Vector3 end = start + new Vector3(0.8f, 0, 0); // total movement in x-axis
        float duration = 1f; // move over 1 second
        float t = 0f;

        while (t < duration)
        {
            cup.transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        cup.transform.position = end;

        // Enable gravity
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
            rend.material.color = Color.white; // Reset to original color (or you can store originalColor if you want)
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
}
