using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private float detectRadius;
    [SerializeField] private int raycastDegree;
    [SerializeField] private float lineRadius;

    private int degreeStep;
    private List<RaycastHit2D> hitList;    
    private List<float> hitAngles;
    
    // Start is called before the first frame update
    void Start()
    {
        degreeStep = 360 / raycastDegree;
        hitList = new List<RaycastHit2D>();
        hitAngles = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {
        hitList.Clear();

        for (float angle = 0f; angle < 360f; angle += degreeStep)
        {
            Vector2 rayDirection = Quaternion.Euler(0, 0, angle) * Vector3.right * detectRadius;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, detectRadius);

            hitList.Add(hit);
            hitAngles.Add(angle);

        }
    }

    public int getRaycastDegree()
    {
        return raycastDegree;
    }

    public List<float> getHitAngles()
    {
        return hitAngles;
    }

    public float getDetectRadius()
    {
        return detectRadius;
    }

    public List<RaycastHit2D> getHitList()
    {
        return hitList;
    }

    public float getLineRadius()
    {
        return lineRadius;
    }
}
