using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject darkMode;
    [SerializeField] private GameObject darkModeOutline;
    [SerializeField] private GameObject lightMode;
    [SerializeField] private GameObject lightModeOutline;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateLightMode()
    {
        darkMode.SetActive(false);
        darkModeOutline.SetActive(false);
        lightMode.SetActive(true);
        lightModeOutline.SetActive(true);
    }
    public void ActivateDarkMode()
    {
        darkMode.SetActive(true);
        darkModeOutline.SetActive(true);
        lightMode.SetActive(false);
        lightModeOutline.SetActive(false);
    }

}
