using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Button informationButton;
    public GameObject information;
    public GameObject[] informationText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadNewScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
        Debug.Log("New scene has been loaded");
    }
    public void displayInformation(int index)
    {
        information.SetActive(true);
        informationText[index].SetActive(true);
    }
    public void hideInformation(int index)
    {
        informationText[index].SetActive(false);
    }
}
