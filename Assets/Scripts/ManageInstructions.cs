using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManageInstructions : MonoBehaviour
{
    private Button button;
    public Button hideInstructionButton;
    public Button showInstructionButton;
    public TextMeshProUGUI instruction;
    private AudioSource speech;
    // Start is called before the first frame update
    void Start()
    {
        button = GetComponent<Button>();
        speech = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowInstructions()
    {
        Debug.Log(gameObject + " was clicked."); 
        showInstructionButton.gameObject.SetActive(false);
        instruction.gameObject.SetActive(true);
        hideInstructionButton.gameObject.SetActive(true);
        speech.Play();
    }
    public void HideInstructions()
    {
        Debug.Log(gameObject + " was clicked.");
        hideInstructionButton.gameObject.SetActive(false);
        showInstructionButton.gameObject.SetActive(true);
        instruction.gameObject.SetActive(false);
    }
}
