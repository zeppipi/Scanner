using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererScript : MonoBehaviour
{
    [SerializeField] private float lineWidths = 1;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Detector detector;
    [SerializeField] private float detectorRadiusMultiplier = 1;

    // Variables from detector
    private int degreeStep;
    private int raycastDegree;
    private float detectRadius;
    private List<RaycastHit2D> hitList;    
    private List<float> hitAngles;

    private List<List<float>> circlePoints = new List<List<float>>();
    private List<List<int>> groupIndex = new List<List<int>>();

    // List o bools to check which part of the circle is detecting something
    private bool[] isDetecting;
    private GameObject[] lineRenderers;

    // Start is called before the first frame update
    void Start()
    {
        // Get raycast degree for degree step
        raycastDegree = detector.getRaycastDegree();
        degreeStep = 360 / raycastDegree;

        // Get radius
        detectRadius = detector.getDetectRadius() * detectorRadiusMultiplier;

        // Get circle points
        circlePoints = GetCirclePoints(raycastDegree, detectRadius);
        isDetecting = new bool[circlePoints.Count];     // Note: isDetecting[-1] = isDetecting[0]
        
        // Start the array with 1
        lineRenderers = new GameObject[1];

        // Do in the start to not get out of range error
        hitAngles = detector.getHitAngles();
        hitList = detector.getHitList();
    }

    // Update is called once per frame
    void Update()
    {
        // Get hit angles and hit list
        hitAngles = detector.getHitAngles();
        hitList = detector.getHitList();

        // Update the bool list depeneding on which part is getting detected
        for(int index = 0; index < hitList.Count; index++)
        {
            // Calculate distance
            float distance = Vector2.Distance(hitList[index].point, transform.position);
            
            if(hitList[index].collider != null && distance <= detectRadius)
            {
                isDetecting[index] = true;
            }
            else
            {
                isDetecting[index] = false;
            }
        }

        // Find the groups
        groupIndex = FindGroups(isDetecting);

        // Reset all lines
        for(int index = 0; index < lineRenderers.Length; index++)
        {
            if (lineRenderers[index] != null)
            {
                lineRenderers[index].GetComponent<LineRenderer>().positionCount = 0;
            }
        }

        // Loop through the groups and create their lines
        List<List<float>> points = new List<List<float>>();     // Create the points
        for(int index = 0; index < groupIndex.Count; index++)
        {
            // Reset points
            points = new List<List<float>>();

            // Check if there is enough space
            if (groupIndex.Count > lineRenderers.Length)
            {
                lineRenderers = DoubleArray(lineRenderers);
            }
            
            for (int innerIndex = 0; innerIndex < groupIndex[index].Count; innerIndex++)
            {
                // Edge case for 0
                if (groupIndex[index][innerIndex] == 0)
                {
                    points.Add(circlePoints[circlePoints.Count - 2]);
                    points.Add(circlePoints[circlePoints.Count - 1]);
                }
                else
                {
                    // Add the point to the list
                    points.Add(circlePoints[groupIndex[index][innerIndex] - 1]);   
                    points.Add(circlePoints[groupIndex[index][innerIndex]]);
                }
                
                // Check if there is a line in this index
                if (lineRenderers[index] == null)
                {
                    // Create the line
                    lineRenderers[index] = CreateLine(points);
                }
                else
                {
                    // Update the line
                    SetLine(points, lineRenderers[index]);
                }
            }
        }
    }

    List<List<int>> FindGroups(bool[] isDetecting)
    {
        /*
            Finds the groups of the bool list
        */

        // List of groups
        List<List<int>> groups = new List<List<int>>();

        // List of indexes
        List<int> indexes = new List<int>();

        // Loop through the bool list
        for(int index = 0; index < isDetecting.Length; index++)
        {
            // If the bool is true
            if(isDetecting[index])
            {
                // Add the index to the list
                indexes.Add(index);
            }
            else
            {
                // If the list is not empty
                if(indexes.Count > 0)
                {
                    // Add the list to the groups
                    groups.Add(indexes);

                    // Clear the list
                    indexes = new List<int>();
                }
            }
        }

        // Get the left over indexes
        if(indexes.Count > 0)
        {
            groups.Add(indexes);
        }

        // Return the groups
        return groups;   
    }
    
    GameObject[] DoubleArray(GameObject[] arr)
    {
        GameObject[] res = new GameObject[arr.Length * 2];
        for(int index = 0; index < arr.Length; index++)
        {
            res[index] = arr[index];
        }
        return res;
    }
    
    void SetLine(List<List<float>> points, GameObject line)
    {
        /*
            Takes a list of points and sets the line renderer
        */

        // Get the line renderer
        LineRenderer lineRend = line.GetComponent<LineRenderer>();

        // Set the positions
        lineRend.positionCount = points.Count;
        for(int innerIndex = 0; innerIndex < points.Count; innerIndex++)
        {
            lineRend.SetPosition(innerIndex, new Vector2(points[innerIndex][0], points[innerIndex][1]));
        }
    }
    
    GameObject CreateLine(List<List<float>> points)
    {
        /*
            Takes a list of points and creates a line renderer
        */

        // Create line renderer
        GameObject lineRenderer = new GameObject("LineRenderer");
        lineRenderer.transform.parent = transform;
        lineRenderer.transform.localPosition = Vector3.zero;
        lineRenderer.transform.localRotation = Quaternion.identity;

        // Add line renderer component
        LineRenderer line = lineRenderer.AddComponent<LineRenderer>();
        line.material = lineMaterial;
        line.startWidth = lineWidths;
        line.endWidth = lineWidths;
        line.positionCount = points.Count;
        line.useWorldSpace = false;

        // Add custom components
        lineRenderer.AddComponent<RotationCancel>();
        // lineRenderer.AddComponent<LineScript>().Constructor(detector, degreeStep);

        // Add positions
        for(int index = 0; index < points.Count; index++)
        {
            line.SetPosition(index, new Vector3(points[index][0], points[index][1], 0));
        }

        // Return
        return lineRenderer;
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
}
