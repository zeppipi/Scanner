using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashSystem : MonoBehaviour
{
    [SerializeField] private Detector detector;
    [SerializeField] private float crashRadius;     // The circle area where you would crash

    private int degreeStep;
    private int raycastDegree;

    private List<RaycastHit2D> hitList;
    private List<float> hitAngles;
    private List<List<float>> circlePoints = new List<List<float>>();

    private Rigidbody2D rb;

    private bool inCrashBool;
    private Vector3 crashPoint;
    
    // Called when script is instantiated
    void Awake()
    {
        // Get the rigidbody
        rb = GetComponent<Rigidbody2D>();
        
        // Get 'detector' variables
        raycastDegree = detector.getRaycastDegree();
        degreeStep = 360 / raycastDegree;
        circlePoints = GetCirclePoints(raycastDegree, crashRadius);
        
        // Prepare the hitlist and hitangles
        hitList = new List<RaycastHit2D>();
        hitAngles = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        Crash();   
    }

    private void Crash()
    {  
        // Get the hitlist and hitangles
        hitList = detector.getHitList();
        hitAngles = detector.getHitAngles();

        // Check if you are in the crash area
        inCrashBool = false;
        for(int index = 0; index < hitList.Count; index++)
        {
            if(hitList[index].collider != null)
            {
                // Find distance
                Vector2 currentPoint = hitList[index].point;
                float hitAndPlayerDist = Vector2.Distance(currentPoint, transform.position);

                if(hitAndPlayerDist <= crashRadius && rb.velocity.magnitude > 0.1f)
                {
                    crashPoint = new Vector3(circlePoints[index][0], circlePoints[index][1], 0);
                    inCrashBool = true;
                    break;
                }
            }
        }
    }

    List<List<float>> GetCirclePoints(int steps, float radius)
    {
        // List of coordinates
        List<float> xCoords = new List<float>();
        List<float> yCoords = new List<float>();

        for(int index = 0; index < steps + 1; index++)
        {
            float currentCircum = (float) index / steps;
            float currentRadian = currentCircum * 2 * Mathf.PI;

            float x = Mathf.Cos(currentRadian) * radius;
            float y = Mathf.Sin(currentRadian) * radius;

            xCoords.Add(x);
            yCoords.Add(y);
        }

        // Return the lines
        List<List<float>> res = new List<List<float>>();
        for(int index = 0; index < steps + 1; index++)
        {
            List<float> curList = new List<float>() {xCoords[index], yCoords[index]};            
            res.Add(curList);
        } 
        return res;
    }

    public bool CrashBool()
    {
        return inCrashBool;
    }

    public Vector3 CrashPoint()
    {
        return crashPoint;
    }
}
