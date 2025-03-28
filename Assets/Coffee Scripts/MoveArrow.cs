using UnityEngine;

public class MoveArrow : MonoBehaviour
{
    [SerializeField] LineRenderer laser;

    public void PointTo(Transform target)
    {
        laser.SetPosition(0, transform.position); // base of the arrow or laser
        laser.SetPosition(1, target.position);     // tip of the arrow goes to button
    }
}