using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveHand : MonoBehaviour
{
    private bool shouldMove = true;

    void Update()
    {
        if (shouldMove)
        {
            MoveForward();
        }
    }

    public void MoveForward()
    {
        transform.Translate(0, 0, 0.05f * Time.deltaTime);
    }

    // Detect trigger or collision with an object tagged "yellow"
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Yellow"))
        {
            shouldMove = false;
            Debug.Log("Touched yellow object. Stopping movement.");
            Debug.Log("Touched yellow. Will destroy hand.");
            Destroy(gameObject);
            Destroy(collision.gameObject);
        }
    }

    // Optional: For trigger-based colliders
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Yellow"))
        {
            shouldMove = false;
            Debug.Log("Entered yellow trigger. Stopping movement.");
            Destroy(gameObject);
        }
        Destroy(other.gameObject);
    }
}