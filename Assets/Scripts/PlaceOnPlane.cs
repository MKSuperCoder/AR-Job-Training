using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceOnPlane : MonoBehaviour
{
    private ARRaycastManager raycastManager;
    private bool objectPlaced = false;

    public GameObject objectToPlace; // Child 3D model
    public Camera arCamera;

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();

        if (objectToPlace != null)
            objectToPlace.SetActive(false); // Hide until placement
    }

    void Update()
    {
        if (objectPlaced)
            return;

        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            // Move parent object to plane
            transform.position = hitPose.position;
            transform.rotation = hitPose.rotation;

            // Activate the child object
            if (objectToPlace != null)
                objectToPlace.SetActive(true);

            objectPlaced = true;
        }
    }
}
