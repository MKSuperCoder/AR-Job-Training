using UnityEngine;
using TMPro;
using System.Collections;

public class CoffeeButton : MonoBehaviour
{
    public enum ButtonType { Strength, Fresh, Clean, Brew }
    private Vector3 originalCupPosition;
    public ButtonType buttonType;
    public CoffeeTraining trainer;
    public TMP_Text strengthText;
    public TMP_Text typeText;
    public GameObject brewedCoffee;      // Shows for 2 seconds
    public GameObject servedCoffee;      // Shown after 2 seconds
    public GameObject cup;

    private int strengthLevel = 0;

    private void Update()
    {
        if (TouchOrClickDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    Debug.Log($"You clicked or touched {gameObject.name}!");
                    OnClick();
                }
            }
        }
    }

    private bool TouchOrClickDown()
    {
        return Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }

    public void OnClick()
    {
        trainer.OnCorrectButtonPressed(gameObject); // Step validation

        switch (buttonType)
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
    }


}
