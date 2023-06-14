using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioClip;
    [Range(0, 1), SerializeField] private float volume;
    [Range(-3, 3), SerializeField] private float thurstPitch = 1;
    [SerializeField] private Controller controller;

    private Rigidbody2D rb;
    [SerializeField] private bool isOn = true;     // This should at some point, be turned off and on programatically
    private bool moving = false;
    
    private float maxSpeed;
    private float maxSpeedProgress;

    // Start is called before the first frame update
    void Start()
    {
        // Get rigidbody
        rb = GetComponent<Rigidbody2D>();

        // Get controller max speed
        maxSpeed = controller.GetMaxSpeed();

        // Play audio clip
        audioClip.Play();
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate max speed progress
        maxSpeedProgress = rb.velocity.magnitude / maxSpeed;
        
        // Detect if moving
        if (rb.velocity.magnitude > 0.1f)
        {
            moving = true;
        }
        else
        {
            moving = false;
        }
        
        // Set volume and pitch
        if (isOn == true)
        {
            audioClip.pitch = 0; 

            if (moving == true)
            {
                audioClip.pitch = maxSpeedProgress * thurstPitch;
            }
        }
        else
        {
            audioClip.volume = 0;
        }
    }
}
