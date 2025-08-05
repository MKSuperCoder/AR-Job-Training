using UnityEngine;

public class ClickAndDrag : MonoBehaviour
{
    private Camera arCamera;
    private Transform selectedObject;
    private Vector3 offset;
    private float dragDistance;
    public GameObject MoveInstruction;
    public GameObject MoveZInstruction;

    private float initialPinchDistance;
    private float initialObjectDistance;

    private float minDistance = 0.2f;
    private float maxDistance = 3f;
    public RestroomCleaningTrainingManager restroomManager;

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

            if (MoveZInstruction != null && !MoveZInstruction.activeSelf)
                MoveZInstruction.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            selectedObject = null;

            if (MoveInstruction != null)
                MoveInstruction.SetActive(true); // Show when not touching
            if (MoveZInstruction != null)
                MoveZInstruction.SetActive(false);
        }
        else if (!Input.GetMouseButton(0) && selectedObject == null)
        {
            // ✅ Show MoveInstruction when not touching and nothing is selected
            if (MoveInstruction != null && !MoveInstruction.activeSelf)
                MoveInstruction.SetActive(true);
        }

#else
        if (Input.touchCount == 0 && selectedObject == null)
        {
            // ✅ Show MoveInstruction when no touch and nothing selected
            if (MoveInstruction != null && !MoveInstruction.activeSelf)
                MoveInstruction.SetActive(true);
        }

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

                        if (MoveZInstruction != null && !MoveZInstruction.activeSelf)
                            MoveZInstruction.SetActive(true);
                    }
                    break;
                case TouchPhase.Ended:
                    selectedObject = null;

                    if (MoveInstruction != null)
                        MoveInstruction.SetActive(true); // Show when finger lifted
                    if (MoveZInstruction != null)
                        MoveZInstruction.SetActive(false);
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

                if (MoveZInstruction != null)
                    MoveZInstruction.SetActive(true);
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
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Draggable") || hit.transform.CompareTag("Brush") || hit.transform.CompareTag("Towel") || hit.transform.CompareTag("Tissue") || hit.transform.CompareTag("Mop"))
            {
                selectedObject = hit.transform;
                dragDistance = Vector3.Distance(arCamera.transform.position, selectedObject.position);
                offset = selectedObject.position - GetWorldPoint(screenPosition, dragDistance);
            }
            if (hit.transform.CompareTag("Brush"))
            {
                restroomManager.imageGuide.SetActive(true);
            }
        }

        if (MoveInstruction != null)
            MoveInstruction.SetActive(false); // Hide once object is picked
    }

    void DragSelectedObject(Vector2 screenPosition)
    {
        Vector3 newWorldPos = GetWorldPoint(screenPosition, dragDistance) + offset;
        selectedObject.position = newWorldPos;

        if (MoveInstruction != null && MoveInstruction.activeSelf)
            MoveInstruction.SetActive(false); // Hide if dragging
    }

    Vector3 GetWorldPoint(Vector2 screenPos, float fixedDistance)
    {
        float distance = Mathf.Clamp(fixedDistance, minDistance, maxDistance);
        return arCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
    }
}
