using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float turnSpeed;
    [SerializeField] private float thurstSpeed;
    [SerializeField, Range(0,1)] private float friction;
    
    [SerializeField] private ParticleSystem thurstParticles;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        var isThursting = thurstParticles.emission;
        isThursting.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        float turnDirection = Input.GetAxis("Horizontal");
        float thurstOn = Input.GetAxis("Jump");

        // Move player
        rb.AddForce(transform.up * thurstOn * thurstSpeed * Time.deltaTime);
        if (Mathf.Abs(rb.angularVelocity * -turnDirection) <= Mathf.Abs(turnSpeed * -turnDirection))
        {
            rb.AddTorque(-turnDirection * turnSpeed * Time.deltaTime);
        }
        else
        {
            rb.angularVelocity = -turnDirection * turnSpeed;
        }

        // Add friction
        if (thurstOn == 0)
        {
            rb.velocity = rb.velocity * friction;
        }
        if (turnDirection == 0)
        {
            rb.angularVelocity = rb.angularVelocity * friction;
        }

        // Turn on/off particles
        var isThursting = thurstParticles.emission;
        if (Mathf.Abs(thurstOn) > 0.1f)
        {
            isThursting.enabled = true;
        }
        else
        {
            isThursting.enabled = false;
        }
    }
}
