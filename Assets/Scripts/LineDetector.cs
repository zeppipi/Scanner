using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDetector : MonoBehaviour
{
    [SerializeField] private float lineWidths = 1;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Detector detector;

    private List<List<float>> circlePoints = new List<List<float>>();
    private int degreeStep;
    private int raycastDegree;
    private float detectRadius;
    
    private List<RaycastHit2D> hitList;
    private List<float> hitAngles;

    // Make the three lines
    private List<GameObject> outerLineRenderer = new List<GameObject>();
    private List<GameObject> middleLineRenderer = new List<GameObject>();
    private List<GameObject> innerLineRenderer = new List<GameObject>();
    private float outerRadius;
    private float middleRadius;
    private float innerRadius;

    // Called when an instance of the script is first created
    void Awake()
    {
        // Create hitlist and hitangles
        hitList = new List<RaycastHit2D>();
        hitAngles = new List<float>();
        
        // Get the degrees for each raycast
        raycastDegree = detector.getRaycastDegree();
        degreeStep = 360 / raycastDegree;

        // Calculate radiuses
        detectRadius = detector.getDetectRadius();
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
        hitList = detector.getHitList();
        hitAngles = detector.getHitAngles();

        for (int index = 0; index < hitList.Count; index++)
        {
            if (hitList[index].collider != null)
            {
                float hitAndPlayerDist = Vector2.Distance(hitList[index].point, transform.position);
                // This is a mess
                if(hitAndPlayerDist > outerRadius)
                {
                    outerLineRenderer[(int) hitAngles[index] / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(true);
                }
                else if(hitAndPlayerDist > middleRadius)
                {
                    middleLineRenderer[(int) hitAngles[index] / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(true);
                }
                else if(hitAndPlayerDist > innerRadius)
                {
                    innerLineRenderer[(int) hitAngles[index] / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(true);
                }
            }
            else
            {
                outerLineRenderer[(int) hitAngles[index] / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(false);
                middleLineRenderer[(int) hitAngles[index] / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(false);
                innerLineRenderer[(int) hitAngles[index] / degreeStep].GetComponent<DetectCollider>().SetLocalActivation(false);
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
}
