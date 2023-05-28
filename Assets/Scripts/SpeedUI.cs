using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    [SerializeField] private TMP_Text speedText;
    
    private GameObject player;
    private Rigidbody2D playerRb;

    void Start()
    {
        player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        string speedTextTemp;

        if (player != null)
        {
            speedTextTemp = playerRb.velocity.magnitude.ToString("F2");
        }
        else
        {
            speedTextTemp = "0";
            
            player = GameObject.FindWithTag("Player");
            playerRb = player.GetComponent<Rigidbody2D>();
        }

        speedText.text = "Speed:\n" + speedTextTemp + " unit/s";
    }
}
