using UnityEngine;

public class MessSpot : MonoBehaviour
{
    public RestroomCleaningTrainingManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Towel"))
        {
            manager.OnMessCleaned(this.gameObject);
        }
    }
}
