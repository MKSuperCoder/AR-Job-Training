using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public void GoToHome()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }
    public void GoToProfile()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("UserProfile");
    }

    public void GoToModuleLibrary()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ModuleLibrary");
    }

    
}
