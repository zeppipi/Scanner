using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private bool remoteActivated = false;   // Line is being activated from another script
    private bool localActivated = false;    // Line is being activated from this script

    private GameObject remoteAgent = null; // The other line this gameobject is controlling
    
    // Start is called before the first frame update
    void Awake()
    {
        this.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        lineRenderer = this.gameObject.GetComponent<LineRenderer>();
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

    public void SetLocalActivation(bool value)
    {
        // These names are getting confusing
        localActivated = value;
    }

    public void SetActivation(bool value)
    {
        remoteActivated = value;
    }

    public void AddAgent(GameObject agent)
    {
        remoteAgent = agent;
    }
}
