using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioClip;
    [SerializeField] private CrashSystem crashSystem;
    [SerializeField] private float maxSpeed;    // The speed where the sound will be full volume
    [SerializeField] private float minSpeed;    // The speed where the sound will be no volume
    [SerializeField, Range(0, 1)] private float maxVolume;   // The max volume of the sound

    private Rigidbody2D rb;
    private float rbSpeed;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get speed
        if (rb.velocity.magnitude > minSpeed)
        {
            rbSpeed = rb.velocity.magnitude;
        }
        else
        {
            rbSpeed = 0;
        }

        // Play sound
        if(crashSystem.CrashBool())
        {
            audioClip.volume = Mathf.Lerp(0f, maxVolume, (Mathf.InverseLerp(0, maxSpeed, rbSpeed)));
            audioClip.Play();
        }
    }
}
