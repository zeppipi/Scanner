using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioClip;
    [SerializeField] private Detector detector;
    [SerializeField] private float minRadius;   // The smallest distance 

    [SerializeField] private float beepPerSecond;
    private float currentTime;
    private bool isPlaying = false;

    private List<RaycastHit2D> hitList;
    private float detectRadius;

    private RaycastHit2D currClosestHit;
    private float bestDistance = 999f;

    // Start is called before the first frame update
    void Start()
    {
        // Prepare the detector clips
        hitList = detector.getHitList();
        detectRadius = detector.getDetectRadius();

        // Prepare the 'beepPerSecond' variable
        currentTime = beepPerSecond;
    }

    // Update is called once per frame
    void Update()
    {
        // Reset best distance
        bestDistance = 999f;

        // Check if sound should play
        if (isPlaying)
        {
            // Countdown
            currentTime -= Time.deltaTime;

            if(currentTime <= 0)
            {
                audioClip.Play();
                currentTime = beepPerSecond;
            }
        }
        
        for (int index = 0; index < hitList.Count; index++)
        {   
            // Calculate distance
            float distance = 0;
            if (hitList[index].distance > 0)
            {
                distance = hitList[index].distance;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    currClosestHit = hitList[index];
                }
            }
        }

        // Check if sound should play
        if (bestDistance < detectRadius && currClosestHit.distance > 0)
        {
            isPlaying = true;
        }
        else
        {
            isPlaying = false;
        }
    }
}
