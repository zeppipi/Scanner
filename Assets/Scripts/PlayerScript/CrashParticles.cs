using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrashParticles : MonoBehaviour
{
    [SerializeField] private CrashSystem crashSystem;
    [SerializeField] private ParticleSystem crashParticles;

    private ParticleSystem.EmissionModule crashParticlesEmission;
    private ParticleSystem.ShapeModule crashParticlesShape;

    private Rigidbody2D rb;

    void Awake()
    {
        // Get the rigidbody
        rb = GetComponent<Rigidbody2D>();

        // Prepare the crash particles
        crashParticlesEmission = crashParticles.emission;
        crashParticlesEmission.enabled = false;

        // Prepare the crash particles shape
        crashParticlesShape = crashParticles.shape;
    }

    void Update() 
    {
        // Get bool and vector
        bool inCrashArea = crashSystem.CrashBool();
        Vector3 crashPoint = crashSystem.CrashPoint();

        // If you are in the crash area, enable the crash particles
        if(inCrashArea)
        {
            crashParticlesShape.position = crashPoint;
            crashParticlesEmission.enabled = true;
        }
        else
        {
            crashParticlesEmission.enabled = false;
        }  
    }
}
