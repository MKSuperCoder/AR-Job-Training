using UnityEngine;

public class TissueBehavior : MonoBehaviour
{
    public RestroomCleaningTrainingManager manager;

    public void OnDroppedInBasket()
    {
        // You can add animation, sound, or destroy the tissue
        manager.OnTissueDroppedInBin();
        Destroy(gameObject); // Optional
    }
}
