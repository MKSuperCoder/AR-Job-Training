using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class ObjectDetection : MonoBehaviour
{
    public ARTrackedObjectManager objectManager; // Manages tracked AR objects
    public GameObject animation;


    void Start()
    {

    }

    void OnEnable()
    {
        // Subscribe to the trackedObjectsChanged event
        objectManager.trackedObjectsChanged += OnTrackedObjectsChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from the trackedObjectsChanged event
        objectManager.trackedObjectsChanged -= OnTrackedObjectsChanged;
    }

    // Event handler for tracked objects changes
    void OnTrackedObjectsChanged(ARTrackedObjectsChangedEventArgs eventArgs)
    {
        // Handle added tracked objects
        foreach (var ARTrackedObject in eventArgs.added)
        {

            Debug.Log("Object Detected: " + ARTrackedObject.referenceObject.name);

            // Check if the detected object is a bottle
            if (ARTrackedObject.referenceObject.name == "Bottle")
            {

                animation.gameObject.SetActive(true);


            }
            if (ARTrackedObject.referenceObject.name == "Box")
            {
                animation.gameObject.SetActive(true);
            }
        }

        // Handle updated tracked objects
        foreach (var ARTrackedObject in eventArgs.updated)
        {


        }

        // Handle removed tracked objects
        foreach (var ARTrackedObject in eventArgs.removed)
        {

        }
    }

    void Update()
    {

    }
}

  