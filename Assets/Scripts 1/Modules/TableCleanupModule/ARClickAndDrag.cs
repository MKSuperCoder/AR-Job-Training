using UnityEngine;

public class ARClickAndDrag : MonoBehaviour
{
    public TaskManager taskManager;
    private Camera arCamera;
    private Transform selectedObject;
    private Vector3 offset;
    private float dragDistance;

    private float initialPinchDistance;
    private float initialObjectDistance;

    private float minDistance = 0.2f;
    private float maxDistance = 3f;

    private float minY = 0.05f;

    void Start()
    {
        arCamera = Camera.main;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            TryPickObject(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && selectedObject != null)
        {
            DragSelectedObject(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            ClearSelection();
        }

#else
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    TryPickObject(touchPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (selectedObject != null)
                    {
                        DragSelectedObject(touchPos);

                    }
                    break;
                case TouchPhase.Ended:
                    selectedObject = null;
                    break;
            }
        }

        // Two-finger pinch to zoom
        if (Input.touchCount == 2 && selectedObject != null)
        {
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            float currentDistance = Vector2.Distance(touch0.position, touch1.position);

            if (touch1.phase == TouchPhase.Began)
            {
                initialPinchDistance = currentDistance;
                initialObjectDistance = Vector3.Distance(arCamera.transform.position, selectedObject.position);

            }
            else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                if (Mathf.Approximately(initialPinchDistance, 0)) return;

                float pinchRatio = currentDistance / initialPinchDistance;
                float newDistance = Mathf.Clamp(initialObjectDistance / pinchRatio, minDistance, maxDistance);

                Vector3 direction = (selectedObject.position - arCamera.transform.position).normalized;
                selectedObject.position = arCamera.transform.position + direction * newDistance;
            }
        }
        // Two-finger twist to rotate
        if (Input.touchCount == 2 && selectedObject != null)
        {  
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);

            // Only apply rotation if both fingers moved
            if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
            {
                Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
                Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;

                float prevAngle = Mathf.Atan2(prevTouch1.y - prevTouch0.y, prevTouch1.x - prevTouch0.x) * Mathf.Rad2Deg;
                float currentAngle = Mathf.Atan2(touch1.position.y - touch0.position.y, touch1.position.x - touch0.position.x) * Mathf.Rad2Deg;

                float angleDelta = currentAngle - prevAngle;

                selectedObject.Rotate(0, -angleDelta, 0); // Rotate around Y-axis

                  // ---- X-axis tilt (forward/backward) ----
                float prevMidY = (prevTouch0.y + prevTouch1.y) * 0.5f;
                float currMidY = (touch0.position.y + touch1.position.y) * 0.5f;
                float deltaY = currMidY - prevMidY;

                float tiltSpeed = 0.1f; // Adjust for sensitivity
                selectedObject.Rotate(-deltaY * tiltSpeed, 0, 0); // Tilt around X-axis (vertical)
            }
        }

#endif
    }

    void TryPickObject(Vector2 screenPosition)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("Draggable") || hit.transform.CompareTag("Tray") || hit.transform.CompareTag("Towel"))
            {
                selectedObject = hit.transform;
                dragDistance = Vector3.Distance(arCamera.transform.position, selectedObject.position);
                offset = selectedObject.position - GetWorldPoint(screenPosition, dragDistance);

                string objName = selectedObject.name.Replace("(Clone)", "").Trim();
                taskManager?.Invoke(nameof(DisableHighlighter), 0f); // safe optional call
                DisableHighlighter(objName);
            }
        }
    }

    void DragSelectedObject(Vector2 screenPosition)
    {
        Vector3 newWorldPos = GetWorldPoint(screenPosition, dragDistance) + offset;

        // Clamp Y
        if (newWorldPos.y < minY)
            newWorldPos.y = minY;

        selectedObject.position = newWorldPos;
    }

    Vector3 GetWorldPoint(Vector2 screenPos, float fixedDistance)
    {
        float distance = Mathf.Clamp(fixedDistance, minDistance, maxDistance);
        return arCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
    }

    void ClearSelection()
    {
        selectedObject = null;
        offset = Vector3.zero;
        dragDistance = 0f;
    }

    void DisableHighlighter(string objName)
    {
        if (taskManager == null) return;

        if (objName.Equals("Cup", System.StringComparison.OrdinalIgnoreCase))
            taskManager.cupHighlighter?.SetActive(false);
        else if (objName.Equals("Straw", System.StringComparison.OrdinalIgnoreCase))
            taskManager.strawHighlighter?.SetActive(false);
        else if (objName.Equals("Burger Box", System.StringComparison.OrdinalIgnoreCase))
            taskManager.burgerBoxHighlighter?.SetActive(false);
        else if (objName.Equals("Fries Box", System.StringComparison.OrdinalIgnoreCase))
            taskManager.friesBoxHighlighter?.SetActive(false);
        else if (objName.StartsWith("Nugget", System.StringComparison.OrdinalIgnoreCase))
        {
            taskManager.nuggetHighlighter1?.SetActive(false);
            taskManager.nuggetHighlighter2?.SetActive(false);
            taskManager.nuggetHighlighter3?.SetActive(false);
            taskManager.nuggetHighlighter4?.SetActive(false);
            taskManager.nuggetHighlighter5?.SetActive(false);
            taskManager.nuggetHighlighter6?.SetActive(false);
        }
        else if (objName.Equals("Nugget Box", System.StringComparison.OrdinalIgnoreCase))
            taskManager.nuggetBoxHighlighter?.SetActive(false);
    }
}
