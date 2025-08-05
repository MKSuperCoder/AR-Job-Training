using UnityEngine;

public class FlushHandleClick : MonoBehaviour
{
    public RestroomCleaningTrainingManager manager;

    void OnMouseDown()
    {
        manager.OnFlushHandleClicked();
    }
}
