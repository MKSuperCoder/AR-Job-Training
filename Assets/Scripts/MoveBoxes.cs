using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBoxes : MonoBehaviour
{
    [SerializeField] GameObject box;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void MoveBox()
    {
        for (int i = 0; i < 7; i++)
        {
            box.transform.Translate(Vector3.up);
        }
        for (int i = 0; i < 7; i++)
        {
            box.transform.Translate(Vector3.left);
        }
        for (int i = 0; i < 7; i++)
        {
            box.transform.Translate(Vector3.down);
        }

    }
}
