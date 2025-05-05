using UnityEngine;

public class DraggableObject : MonoBehaviour
{
    private Camera cam;
    private bool dragging = false;
    private Vector3 offset;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.touchSupported)
        {
            HandleTouch();
        }
        else
        {
            HandleMouse();
        }
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == GetComponent<Collider>())
            {
                offset = transform.position - hit.point;
                dragging = true;
            }
        }

        if (dragging && Input.GetMouseButton(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPos = hit.point + offset;
                transform.position = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        Ray ray = cam.ScreenPointToRay(touch.position);

        if (touch.phase == TouchPhase.Began)
        {
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == GetComponent<Collider>())
            {
                offset = transform.position - hit.point;
                dragging = true;
            }
        }

        if (dragging && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
        {
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPos = hit.point + offset;
                transform.position = new Vector3(targetPos.x, transform.position.y, targetPos.z);
            }
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            dragging = false;
        }
    }
}
