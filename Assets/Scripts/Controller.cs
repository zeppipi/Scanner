using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float turnSpeed;
    [SerializeField] private float thurstSpeed;
    // [SerializeField] private GameObject camera;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
        // rb.angularVelocity = -turnDirection * turnSpeed * Time.deltaTime;
        // transform.Rotate(0, 0, -turnDirection * turnSpeed * Time.deltaTime);

        // var impulse = (angularChangeInDegrees * Mathf.Deg2Rad) * body.inertia;

        // body.AddTorque(impulse, ForceMode2D.Impulse);

        // Move camera
        // camera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
