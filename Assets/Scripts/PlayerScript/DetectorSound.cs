using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectorSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioClip;
    [SerializeField] private Detector detector;
    [SerializeField] private float minRadius;   // The smallest distance 

    [SerializeField] private float lowBeepsPerSecond;
    [SerializeField] private float highBeepsPerSecond;
    private float lowTimeBetweenBeep;
    private float highTimeBetweenBeep;
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

        // Prepare the 'beepPerSecond' variables
        lowTimeBetweenBeep = 1 / lowBeepsPerSecond;
        highTimeBetweenBeep = 1 / highBeepsPerSecond;
        currentTime = lowTimeBetweenBeep;
    }

    // Update is called once per frame
    void Update()
    {
        // Reset best distance
        bestDistance = 999f;
        
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

        // Make the beeps quiter when its far away
        audioClip.volume = Mathf.Lerp(0.1f, 1f, (Mathf.InverseLerp(detectRadius, minRadius, bestDistance)));

        // Pan the sound depending on the direction of the closest hit
        if (currClosestHit.distance > 0 && currClosestHit.distance != 0)
        {
            audioClip.panStereo = Mathf.Lerp(1f, -1f, (Mathf.InverseLerp(-1f, 1f, currClosestHit.normal.x)));
        }
        
        // Check if sound should play
        if (isPlaying)
        {
            // Countdown
            currentTime -= Time.deltaTime;

            if(currentTime <= 0)
            {
                audioClip.Play();
                
                // Dynamically calculate how fast the beeps should play depending on the distance
                currentTime = Mathf.Lerp(lowTimeBetweenBeep, highTimeBetweenBeep, (Mathf.InverseLerp(detectRadius, minRadius, bestDistance)));
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
