using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ImageDetection : MonoBehaviour
{
    public ARTrackedImageManager imageManager; // Manages tracked AR images
    public GameObject animation;

    void OnEnable()
    {
        // Subscribe to the trackedImagesChanged event
        imageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        // Unsubscribe from the trackedImagesChanged event
        imageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    // Event handler for tracked images changes
    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Handle newly detected images
        foreach (var ARTrackedImage in eventArgs.added)
        {
            Debug.Log("Image Detected: " + ARTrackedImage.referenceImage.name);

            // Check if the detected image is "Tissue"
            if (ARTrackedImage.referenceImage.name == "Tissue")
            {
                animation.gameObject.SetActive(true);
            }
        }

        // Handle updated images (if needed)
        foreach (var ARTrackedImage in eventArgs.updated)
        {
            // You can add logic to update animations when tracking changes
        }

        // Handle removed images (if needed)
        foreach (var ARTrackedImage in eventArgs.removed)
        {
            animation.gameObject.SetActive(false);
        }
    }
}
