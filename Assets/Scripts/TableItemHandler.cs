using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TableItemHandler : MonoBehaviour
{
    public GameObject[] hintPlanes; // Hint planes for each item (plate, spoon, etc.)
    private GameObject currentHint;
    private Camera arCamera;
    private GameObject draggingItem;

    void Start()
    {
        arCamera = Camera.main;
        HideAllHints();
    }

    void Update()
    {
        if (draggingItem)
        {
            MoveItemWithTouch(draggingItem);
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.CompareTag("Plate") || hit.collider.CompareTag("Utensil"))
                {
                    draggingItem = hit.collider.gameObject;
                    ShowHintForItem(draggingItem.tag);
                }
            }
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended && draggingItem)
        {
            TryPlaceItem(draggingItem);
        }
    }

    void MoveItemWithTouch(GameObject item)
    {
        Ray ray = arCamera.ScreenPointToRay(Input.GetTouch(0).position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            item.transform.position = hit.point + Vector3.up * 0.01f; // Slightly above surface
        }
    }

    void TryPlaceItem(GameObject item)
    {
        if (currentHint && Vector3.Distance(item.transform.position, currentHint.transform.position) < 0.05f)
        {
            item.transform.position = currentHint.transform.position;
            currentHint.SetActive(false); // Hide the hint after placing
        }

        draggingItem = null;
        currentHint = null;
    }

    void ShowHintForItem(string tag)
    {
        HideAllHints();

        if (tag == "Plate")
            currentHint = hintPlanes[0];
        else if (tag == "Spoon")
            currentHint = hintPlanes[1];
        else if (tag == "Fork")
            currentHint = hintPlanes[2];

        if (currentHint)
            currentHint.SetActive(true);
    }

    void HideAllHints()
    {
        foreach (GameObject hint in hintPlanes)
            hint.SetActive(false);
    }
}
