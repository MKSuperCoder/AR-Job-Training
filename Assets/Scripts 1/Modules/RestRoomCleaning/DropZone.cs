using System.Collections.Generic;
using UnityEngine;

public class DropZone : MonoBehaviour
{
    public RestroomCleaningTrainingManager manager;

    private HashSet<GameObject> droppedTissues = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tissue") && !droppedTissues.Contains(other.gameObject))
        {
            droppedTissues.Add(other.gameObject);

            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
            }

            manager.OnTissueDroppedInBin();
        }
    }
}
