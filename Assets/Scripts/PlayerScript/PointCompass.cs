using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCompass : MonoBehaviour
{
    [SerializeField] private float compassRadius;
    [SerializeField] private GameObject compassNeedle;
    [SerializeField, Range(0,1)] private float lerpValue;

    private GameObject target;
    private Vector3 targetPosition;

    // Instantiate compass needle as a child of the player
    void Awake()
    {
        compassNeedle = Instantiate(compassNeedle, transform.position, Quaternion.identity);
        PointCompassAux auxScript = compassNeedle.AddComponent<PointCompassAux>();
        
        auxScript.setRadius(compassRadius);
        auxScript.setParent(this.gameObject);
        auxScript.setLerp(lerpValue);
    }
    
}
