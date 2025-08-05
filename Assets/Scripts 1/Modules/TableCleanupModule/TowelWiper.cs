using UnityEngine;

public class TowelWiper : MonoBehaviour
{
    public TaskManager taskManager; 

    void OnTriggerStay(Collider other)
    {
        // Only wipe objects with the "Mess" or "Liquid" tag
        if (other.CompareTag("Mess") || other.CompareTag("Liquid"))
        {
            string objName = other.name.Replace("(Clone)", "").Trim();

            // Optional: disable the correct highlighter
            if (taskManager != null)
            {
                switch (objName)
                {
                    case "Nugget1":
                        taskManager.nuggetHighlighter1?.SetActive(false);
                        break;
                    case "Nugget2":
                        taskManager.nuggetHighlighter2?.SetActive(false);
                        break;
                    case "Nugget3":
                        taskManager.nuggetHighlighter3?.SetActive(false);
                        break;
                    case "Nugget4":
                        taskManager.nuggetHighlighter4?.SetActive(false);
                        break;
                    case "Nugget5":
                        taskManager.nuggetHighlighter5?.SetActive(false);
                        break;
                    case "Nugget6":
                        taskManager.nuggetHighlighter6?.SetActive(false);
                        break;
                }
            }

            
            Destroy(other.gameObject);

            
            taskManager?.HandleTask2Step(objName);
        }
    }
}
