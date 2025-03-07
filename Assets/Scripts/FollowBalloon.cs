using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBalloon : MonoBehaviour
{
    public GameObject blueBalloon;
    public GameObject redBalloon;
    public GameObject greenBalloon;
    public GameObject blueLabel;
    public GameObject redLabel;
    public GameObject greenLabel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        blueLabel.transform.position = blueBalloon.transform.position;
        redLabel.transform.position = redBalloon.transform.position;
        greenLabel.transform.position = greenLabel.transform.position;

    }
}
