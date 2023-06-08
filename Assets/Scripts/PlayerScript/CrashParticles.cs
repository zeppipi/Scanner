using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashParticles : MonoBehaviour
{
    // Note: crashing should have a minimum speed
    
    [SerializeField] private Detector detector;
    [SerializeField] private ParticleSystem crashParticles;

    [SerializeField] private float crashRadius;     // The circle area where you would crash
    [SerializeField] private Vector2 offSet;        // The offset of the crash area

    private ParticleSystem.EmissionModule crashParticlesEmission;
    private int degreeStep;
    private int raycastDegree;

    private List<RaycastHit2D> hitList;
    private List<float> hitAngles;
    private List<List<float>> circlePoints = new List<List<float>>();

    void Awake()
    {
        // Prepare the hitlist and hitangles
        hitList = new List<RaycastHit2D>();
        hitAngles = new List<float>();

        // Prepare the crash particles
        crashParticlesEmission = crashParticles.emission;
        crashParticlesEmission.enabled = false;
    }

    void Update() 
    {
        // Get the hitlist and hitangles
        hitList = detector.getHitList();
        hitAngles = detector.getHitAngles();

        // Check if you are in the crash area
        bool inCrashArea = false;
        for(int index = 0; index < hitList.Count; index++)
        {
            if(hitList[index].collider != null)
            {
                // Find distance
                Vector2 currentPoint = hitList[index].point;
                currentPoint += offSet;
                float hitAndPlayerDist = Vector2.Distance(currentPoint, transform.position);

                if(hitAndPlayerDist <= crashRadius)
                {
                    inCrashArea = true;
                    break;
                }
            }
        }

        // If you are in the crash area, enable the crash particles
        if(inCrashArea)
        {
            crashParticlesEmission.enabled = true;
        }
        else
        {
            crashParticlesEmission.enabled = false;
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
}
