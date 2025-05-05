using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShootScript : MonoBehaviour
{
    [SerializeField] GameObject arCamera;
    [SerializeField] GameObject smoke;
    public TMP_Text scoreText;
    private int score = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Shoot()
    {
        RaycastHit hit;

        if (Physics.Raycast(arCamera.transform.position, arCamera.transform.forward, out hit))
        {
            Destroy(hit.transform.gameObject);
            Instantiate(smoke, hit.point, Quaternion.LookRotation(hit.normal));
            score++;
            UpdateScoreText();
        }
    }
    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + score.ToString(); // Update UI correctly
    }
}
