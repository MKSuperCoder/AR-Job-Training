using UnityEngine;

public class SceneOrientation : MonoBehaviour
{
    public enum Orientation
    {
        Portrait,
        LandscapeLeft,
        LandscapeRight
    }

    public Orientation desiredOrientation = Orientation.Portrait;

    void Start()
    {
        switch (desiredOrientation)
        {
            case Orientation.Portrait:
                Screen.orientation = ScreenOrientation.Portrait;
                break;
            case Orientation.LandscapeLeft:
                Screen.orientation = ScreenOrientation.LandscapeLeft;
                break;
            case Orientation.LandscapeRight:
                Screen.orientation = ScreenOrientation.LandscapeRight;
                break;
        }
    }
}
