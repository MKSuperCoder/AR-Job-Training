using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ModuleManager : MonoBehaviour
{
    public GameObject tray;
    public TMP_Text messageText;
    public string trayTag = "Tray";
    public string draggableTag = "Draggable";
    public string towelTag = "Towel";
    private Transform heldObject = null;
    private Vector3 objectOffset;
    private readonly HashSet<Transform> placedObjects = new HashSet<Transform>();
    private List<string> mistakes = new List<string>();

    public void LogMistake(string message)
    {
        mistakes.Add(message);
    }

    public List<string> GetMistakes()
    {
        return mistakes;
    }

    public TaskManager taskManager; // Assign in Inspector

    void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
            TryPickObject(Input.mousePosition);
        else if (Input.GetMouseButton(0) && heldObject != null && !placedObjects.Contains(heldObject))
            DragHeldObject(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
            CheckTrayPlacement();
    }

    void HandleTouch()
    {
        if (Input.touchCount != 1) return;
        Touch touch = Input.GetTouch(0);
        switch (touch.phase)
        {
            case TouchPhase.Began:
                TryPickObject(touch.position);
                break;
            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                if (heldObject != null && !placedObjects.Contains(heldObject))
                    DragHeldObject(touch.position);
                break;
            case TouchPhase.Ended:
                CheckTrayPlacement();
                break;
        }
    }

    void TryPickObject(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag(draggableTag) || hit.transform.CompareTag(trayTag) || hit.transform.CompareTag(towelTag)) 
            {
                heldObject = hit.transform;
                float distance = Vector3.Distance(Camera.main.transform.position, heldObject.position);
                objectOffset = heldObject.position - Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
            }
        }
    }

    void DragHeldObject(Vector2 screenPos)
    {
        float distance = Vector3.Distance(Camera.main.transform.position, heldObject.position);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));
        heldObject.position = worldPos + objectOffset;

    }

    void CheckTrayPlacement()
    {
        if (heldObject == null || taskManager == null) return;

        Collider trayCol = tray.GetComponent<Collider>();
        Collider objCol = heldObject.GetComponent<Collider>();

        if (trayCol != null && objCol != null && trayCol.bounds.Intersects(objCol.bounds))
        {
            string objectName = heldObject.name.Replace("(Clone)", "").Trim();

            // ✅ Start with base list (Task 1 objects)
            HashSet<string> validPlacementObjects = new HashSet<string> {
                "Cup", "Straw", "Burger Box", "Fries Box"
            };

            // ✅ If Task 2, include Task 2-specific additions without losing Task 1 objects
            if (taskManager.IsTask2())
            {
                validPlacementObjects.Add("Nugget Box");
            }

            // ✅ Check if this object is valid for placement
            if (validPlacementObjects.Contains(objectName))
            {
                if (!placedObjects.Contains(heldObject))
                {
                    placedObjects.Add(heldObject);
                    heldObject.SetParent(tray.transform);
                    heldObject.localPosition = new Vector3(0, 0.05f + 0.02f * placedObjects.Count, 0);

                    Collider col = heldObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    heldObject.tag = "Untagged";

                    ShowMessage($"You placed the {objectName} on the tray.");

                    // Validate only if it’s the correct step
                    taskManager.StepCompleted(objectName);
                }
                /*if (!placedObjects.Contains(heldObject))
                {
                    placedObjects.Add(heldObject);
                    heldObject.SetParent(tray.transform);
                    heldObject.position = tray.transform.position + new Vector3(0, 0.05f + 0.02f * placedObjects.Count, 0);

                    // Disable re-dragging
                    Collider col = heldObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                    heldObject.tag = "Untagged";

                    ShowMessage($"Well done! You placed the {objectName} on the tray.");
                    taskManager.StepCompleted(objectName);
                } */
            }
        }

        heldObject = null;
    }



    void ShowMessage(string text)
    {
        if (messageText != null)
        {
            messageText.text = text;
            CancelInvoke(nameof(ClearMessage));
            Invoke(nameof(ClearMessage), 3f);
        }
    }

    void ClearMessage()
    {
        if (messageText != null)
            messageText.text = "";
    }
    
}
