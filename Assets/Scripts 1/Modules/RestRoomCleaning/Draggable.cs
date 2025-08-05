using UnityEngine;

public class Draggable : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Camera cam;

    private float pinchDistanceLastFrame = 0f;
    private float pinchSpeed = 0.5f;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        HandlePinchZoom();
    }

    void OnMouseDown()
    {
        if (Input.touchCount <= 1)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPos();
        }
    }

    void OnMouseDrag()
    {
        if (isDragging && Input.touchCount <= 1)
        {
            transform.position = GetMouseWorldPos() + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = Vector3.Distance(cam.transform.position, transform.position); // accurate depth
        return cam.ScreenToWorldPoint(mousePoint);
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            float currentPinchDistance = Vector2.Distance(touch0.position, touch1.position);

            if (pinchDistanceLastFrame != 0)
            {
                float delta = currentPinchDistance - pinchDistanceLastFrame;

                // Move along camera forward/backward
                Vector3 moveDirection = cam.transform.forward * delta * pinchSpeed * Time.deltaTime;
                transform.position += moveDirection;
            }

            pinchDistanceLastFrame = currentPinchDistance;
        }
        else
        {
            pinchDistanceLastFrame = 0f;
        }
    }
}
