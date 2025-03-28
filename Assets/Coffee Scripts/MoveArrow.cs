using UnityEngine;
using System.Collections;

public class MoveArrow : MonoBehaviour
{
    public GameObject arrowObject;       // The actual arrow GameObject
    public float moveDuration = 0.5f;    // How fast it moves to the button
    public float visibleTime = 1f;       // How long it stays visible

    private Coroutine arrowRoutine;

    public void PointTo(GameObject target)
    {
        if (arrowRoutine != null)
            StopCoroutine(arrowRoutine);

        arrowRoutine = StartCoroutine(MoveAndShowArrow(target.transform.position));
    }

    private IEnumerator MoveAndShowArrow(Vector3 targetPos)
    {
        arrowObject.SetActive(true);

        Vector3 startPos = arrowObject.transform.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            arrowObject.transform.position = Vector3.Lerp(startPos, targetPos + Vector3.up * 0.2f, t);
            arrowObject.transform.LookAt(targetPos); // Rotate to face the button (optional)
            yield return null;
        }

        yield return new WaitForSeconds(visibleTime);

        arrowObject.SetActive(false);
    }
}
