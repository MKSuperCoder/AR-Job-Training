using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonScript : MonoBehaviour
{
    [SerializeField] float speed = 0.2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    //ik
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * speed);
    }
}
