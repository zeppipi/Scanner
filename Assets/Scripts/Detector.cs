using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private float detectRadius;
    [SerializeField] private int raycastDegree;
    [SerializeField] private float lineWidths = 1;
    [SerializeField] private Material lineMaterial;
    
    private int degreeStep = 1;
    private Collider2D[] detectedItems;
    private List<List<float>> circlePoints = new List<List<float>>();

    // Make the three lines
    private List<GameObject> outerLineRenderer = new List<GameObject>();
    private List<GameObject> middleLineRenderer = new List<GameObject>();
    private List<GameObject> innerLineRenderer = new List<GameObject>();

    private float outerRadius;
    private float middleRadius;
    private float innerRadius;
    
    // Start is called before the first frame update
    void Awake()
    {
        // Get the degrees for each raycast
        degreeStep = 360 / raycastDegree;

        // Calculate radiuses
        outerRadius = detectRadius;
        middleRadius = detectRadius / 1.5f;
        innerRadius = detectRadius / 3;

        // Create lines
        circlePoints = GetCirclePoints(raycastDegree, outerRadius);
        outerLineRenderer = CreateLines(circlePoints);

        circlePoints = GetCirclePoints(raycastDegree, middleRadius);
        middleLineRenderer = CreateLines(circlePoints, outerLineRenderer);

        circlePoints = GetCirclePoints(raycastDegree, innerRadius);
        innerLineRenderer = CreateLines(circlePoints, middleLineRenderer);
    }

    // Update is called once per frame
    void Update()
    {
        detectedItems = Physics2D.OverlapCircleAll(transform.position, detectRadius);
        DrawRaycastHit(); 
    }

    void DrawRaycastHit()
    {        
        for (float angle = 0f; angle < 360f; angle += degreeStep)
        {

            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right * detectRadius;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, detectRadius);

            if (hit.collider != null)
            {
                float hitAndPlayerDist = Vector2.Distance(hit.point, transform.position);
                
                // This is a mess
                if(hitAndPlayerDist > outerRadius)
                {
                    outerLineRenderer[(int) angle / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(true);
                }
                else if(hitAndPlayerDist > middleRadius)
                {
                    middleLineRenderer[(int) angle / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(true);
                }
                else if(hitAndPlayerDist > innerRadius)
                {
                    innerLineRenderer[(int) angle / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(true);
                }
            }
            else
            {
                outerLineRenderer[(int) angle / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(false);
                middleLineRenderer[(int) angle / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(false);
                innerLineRenderer[(int) angle / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(false);
            }

        }
    }

    List<List<float>> GetCirclePoints(int steps, float radius)
    {
        /*
            Takes a line renderer and draws a circle
        */

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

        // Rotate lines
        float desiredAngle = (360 / raycastDegree) / 2;
        for(int index = 0; index < steps + 1; index++)
        {
            float fixedRadian = desiredAngle * Mathf.Deg2Rad;

            float x = xCoords[index] * Mathf.Cos(fixedRadian) - yCoords[index] * Mathf.Sin(fixedRadian);
            float y = xCoords[index] * Mathf.Sin(fixedRadian) + yCoords[index] * Mathf.Cos(fixedRadian);

            xCoords[index] = x;
            yCoords[index] = y;
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

    List<GameObject> CreateLines(List<List<float>> points, List<GameObject> lineAgents = null)
    {
        /*
            Creates a shit load of line renderer to dissect the circle from 'points' into their own
            section
        */

        List<GameObject> res = new List<GameObject>();

        for (int index = 1; index < points.Count; index++)
        {
            // Create child object
            GameObject curObject = new GameObject();
            curObject.transform.parent = this.gameObject.transform;
            curObject.transform.localPosition = new Vector3(0, 0, 0);   // Well, that was extremely annoying

            // Create appropiate line
            LineRenderer curLine = curObject.AddComponent<LineRenderer>();
            curLine.useWorldSpace = false;
            curLine.widthMultiplier = lineWidths;
            curLine.material = lineMaterial;
            Vector3 posOne = new Vector3(points[index][0], points[index][1], 0);
            Vector3 posZero = new Vector3(points[index - 1][0], points[index - 1][1], 0);

            // Set line position
            curLine.SetPosition(0, posZero);
            curLine.SetPosition(1, posOne);

            // Add necessary components for the line gameobject
            curObject.AddComponent<RotationCancel>();
            curObject.AddComponent<DetectCollider>();

            // Setters for the detect collider script
            if (lineAgents != null)
            {
                curObject.GetComponent<DetectCollider>().AddAgent(lineAgents[index - 1]);
            }

            res.Add(curObject);
        }

        return res;
    }

    // Gizmos for debug
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
