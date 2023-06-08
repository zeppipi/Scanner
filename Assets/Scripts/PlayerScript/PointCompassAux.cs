using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCompassAux : MonoBehaviour
{
    private GameObject parentGameObject;
    
    private GameObject pointGameObject;
    private Vector3 pointPosition;
    private float radius;
    private float lerpValue;
    
    // Update is called once per frame
    void Update()
    {   
        // Find the location to point
        if (pointGameObject == null)
        {
            pointGameObject = GameObject.FindWithTag("Point");
        }
        else
        {
            pointPosition = pointGameObject.transform.position;
        }

        // Calculate the position relative to parent to face the point
        float angle = 0f;
        if (parentGameObject != null)
        {
            angle = Mathf.Atan2(parentGameObject.transform.position.y - pointPosition.y, parentGameObject.transform.position.x - pointPosition.x);
            angle += 90 * Mathf.Deg2Rad;    // I dont understand circles
        }
        
        float xPos = radius * Mathf.Sin(-angle);
        float yPos = radius * Mathf.Cos(-angle);
        
        // Move the needle to the calculated position
        Vector3 newPos = new Vector3(xPos, yPos, 0) + parentGameObject.transform.position;
        transform.position = Vector3.Lerp(transform.position, newPos, lerpValue);
        transform.rotation = Quaternion.Euler(0,0, angle * Mathf.Rad2Deg);

        // Make the needle's visibility depend on the distance to the point
        float distance = Vector3.Distance(parentGameObject.transform.position, pointPosition);
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color temp = sprite.color;
        temp.a = distance - radius;
        sprite.color = temp;

    }

    public void setRadius(float radius)
    {
        this.radius = radius;
    }

    public void setParent(GameObject parent)
    {
        this.parentGameObject = parent;
    }

    public void setLerp(float lerp)
    {
        this.lerpValue = lerp;
    }
}
