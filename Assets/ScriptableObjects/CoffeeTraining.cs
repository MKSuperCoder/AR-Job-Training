using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoffeeTraining : MonoBehaviour
{
    [SerializeField] RequestSO[] requests;
    [SerializeField] TMP_Text requestText;
    int requestIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        DisplayRequest();
    }
    public void DisplayRequest()
    {
        requestText.text = requests[requestIndex].GetRequest();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
