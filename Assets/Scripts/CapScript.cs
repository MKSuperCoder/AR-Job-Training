using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            transform.Rotate(Vector3.forward * Time.deltaTime * 20);
        }
        for (int i = 0; i < 3; i++)
        {
            transform.Translate(Vector3.up * Time.deltaTime * 0.2f);
        }
        Destroy(gameObject);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
