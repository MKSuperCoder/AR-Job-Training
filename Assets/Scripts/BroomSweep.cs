using UnityEngine;

public class BroomSweep : MonoBehaviour
{
    public float sweepAngle = 15f; // Small rotation for natural sweeping
    public float sweepSpeed = 2f;  // Speed of sweeping motion
    public float sideMovement = 0.2f; // Small sideways movement to enhance realism

    private Quaternion startRotation;
    private Vector3 startPosition;

    void Start()
    {
        startRotation = transform.rotation; // Store initial rotation
        startPosition = transform.position; // Store initial position
    }

    void Update()
    {
        // Rotate slightly forward and backward for sweeping
        float angleOffset = Mathf.Sin(Time.time * sweepSpeed) * sweepAngle;
        transform.rotation = startRotation * Quaternion.Euler(angleOffset, 0, 0);

        // Small sideways movement for a more natural sweeping effect
        float sideOffset = Mathf.Sin(Time.time * sweepSpeed) * sideMovement;
        transform.position = startPosition + transform.right * sideOffset;
    }
}
