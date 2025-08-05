using UnityEngine;

public class HighlightTrigger : MonoBehaviour
{
    public TutorialModuleManager manager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Draggable")) 
        {
            
            manager.OnObjectPlacedCorrectly(other.gameObject, this.gameObject);
        }
    }
}
