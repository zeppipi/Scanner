using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollider : MonoBehaviour
{
    private CircleCollider2D collider;
    private LineRenderer lineRenderer;
    private bool remoteActivated = false;   // Line is being activated from another script
    private bool localActivated = false;    // Line is being activated from this script

    private GameObject remoteAgent = null; // The other line this gameobject is controlling
    
    // Start is called before the first frame update
    void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        collider = this.gameObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;

        lineRenderer = this.gameObject.GetComponent<LineRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        localActivated = true;
        
        if (remoteAgent != null)
        {
            remoteAgent.GetComponent<DetectCollider>().SetActivation(true);
        }   
    }

    void OnTriggerExit2D(Collider2D other) 
    {
        localActivated = false;

        if (remoteAgent != null)
        {
            remoteAgent.GetComponent<DetectCollider>().SetActivation(false);
        }  
    }
    
    // Update is called once per frame
    void Update()
    {
        lineRenderer.enabled = localActivated || remoteActivated;
        
        if (remoteAgent != null)
        {
            remoteAgent.GetComponent<DetectCollider>().SetActivation(localActivated || remoteActivated);
        }
    }

    public void SetActivation(bool value)
    {
        remoteActivated = value;
    }

    public void AddAgent(GameObject agent)
    {
        remoteAgent = agent;
    }

    public void SetRadius(float radius)
    {
        collider.radius = radius;
    }
}
