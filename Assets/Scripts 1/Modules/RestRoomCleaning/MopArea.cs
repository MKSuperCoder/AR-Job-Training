using UnityEngine;

public class MopArea : MonoBehaviour
{
    public RestroomCleaningTrainingManager manager;
    private bool mopped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!mopped && other.CompareTag("Mop"))
        {
            mopped = true;
            manager.OnFloorMopped(gameObject);

        }
    }
}
