using UnityEngine;
using TMPro;

public class CoffeeButton : MonoBehaviour
{
    public enum ButtonType { Strength, Fresh, Clean, Brew }

    public ButtonType buttonType;
    public CoffeeTraining trainer;
    public TMP_Text strengthText;
    public TMP_Text typeText;
    public GameObject brewedCoffee;

    private int strengthLevel = 0;

    private void Update()
    {
        if (TouchOrClickDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    OnClick();
                }
            }
        }
    }

    private bool TouchOrClickDown()
    {
        return Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
    }

    public void OnClick()
    {
        trainer.OnCorrectButtonPressed(gameObject); // Step validation

        switch (buttonType)
        {
            case ButtonType.Strength:
                strengthLevel++;
                if (strengthText != null)
                    strengthText.text = $"Strength: {strengthLevel}";
                break;

            case ButtonType.Fresh:
                if (typeText != null)
                    typeText.text = "Type: Fresh";
                break;

            case ButtonType.Clean:
                if (typeText != null)
                    typeText.text = "Type: Clean";
                break;

            case ButtonType.Brew:
                if (brewedCoffee != null)
                    brewedCoffee.SetActive(true);
                break;
        }
    }
}
