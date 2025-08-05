using UnityEngine;
using System.Collections;

public class SequenceController : MonoBehaviour
{
    public GameObject coffee1;
    public GameObject coffee2;
    public GameObject cup;
    public GameObject hand;

    private bool shouldMove = true;
    private bool hasStartedSequence = false; // to avoid starting the sequence multiple times

    void Update()
    {
        if (shouldMove && hand != null)
        {
            MoveForward();
        }

        // If the hand is destroyed, start the animation sequence (only once)
        if (!hasStartedSequence && hand == null)
        {
            hasStartedSequence = true;
            StartCoffeeSequence();
        }
    }

    public void MoveForward()
    {
        hand.transform.Translate(0, 0, 0.05f * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Yellow"))
        {
            HandleCollision(collision.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Yellow"))
        {
            HandleCollision(other.gameObject);
        }
    }

    private void HandleCollision(GameObject yellowObject)
    {
        shouldMove = false;

        if (hand != null)
        {
            Debug.Log("Touched yellow. Destroying hand.");
            Destroy(hand);
        }

        Destroy(yellowObject);
    }

    public void StartCoffeeSequence()
    {
        StartCoroutine(CoffeeSequence());
    }

    private IEnumerator CoffeeSequence()
    {
        coffee1.SetActive(true);
        yield return new WaitForSeconds(2f);

        coffee1.SetActive(false);
        coffee2.SetActive(true);

        MoveCup();
    }

    public void MoveCup()
    {
        StartCoroutine(MoveCupRoutine());
    }

    private IEnumerator MoveCupRoutine()
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

}
