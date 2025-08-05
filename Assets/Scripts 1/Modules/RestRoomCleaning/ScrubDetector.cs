using UnityEngine;

public class ScrubDetector : MonoBehaviour
{
    public RestroomCleaningTrainingManager manager;
    private float scrubTime = 0f;
    public float requiredTime = 3f;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Brush"))
        {
            scrubTime += Time.deltaTime;

            if (scrubTime >= requiredTime)
            {
                manager.OnToiletBrushed();
                Destroy(gameObject); // Prevent re-trigger
            }
        }
    }
}
