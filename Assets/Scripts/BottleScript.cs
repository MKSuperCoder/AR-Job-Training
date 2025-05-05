using System.Collections;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BottleScript : MonoBehaviour
{
    public GameObject water;
    public GameObject cap;

    private void Start()
    {
        StartCoroutine(BottleSequence());
    }

    private IEnumerator BottleSequence()
    {
        // Move bottle up smoothly
        float moveTime = 1.5f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, 5, -1);
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos; // Ensure final position

        // Rotate bottle
        float rotateTime = 2f;
        elapsedTime = 0;
        while (elapsedTime < rotateTime)
        {
            transform.Rotate(Vector3.right * Time.deltaTime * 60);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Rotate and move cap before destroying it
        float capMoveTime = 1f;
        elapsedTime = 0;
        Vector3 capStartPos = cap.transform.position;
        Vector3 capEndPos = capStartPos + new Vector3(0, 0.5f, 0);

        while (elapsedTime < capMoveTime)
        {
            cap.transform.Rotate(Vector3.forward * Time.deltaTime * 200);
            cap.transform.position = Vector3.Lerp(capStartPos, capEndPos, elapsedTime / capMoveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(cap);

        // Instantiate water
        Instantiate(water, new Vector3(0.2967482f, 0.3912406f, -0.06359005f), Quaternion.identity);
    }
}
