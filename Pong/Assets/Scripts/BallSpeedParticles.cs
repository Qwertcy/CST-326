using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // guarantees a rigidbody exists
public class BallSpeedParticles : MonoBehaviour // scales particle emission based on ball speed
{
    [Header("References")]
    [SerializeField] private ParticleSystem particles; // particle system to drive (assign in inspector)

    [Header("Emission")]
    [SerializeField] private float speedAtMaxEmission = 25f;
    [SerializeField] private float minEmissionRate = 0f;
    [SerializeField] private float maxEmissionRate = 80f;

    [Header("Particle Start Speed")]
    [SerializeField] private float minParticleStartSpeed = 0.5f;
    [SerializeField] private float maxParticleStartSpeed = 6f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (particles == null) particles = GetComponentInChildren<ParticleSystem>(); //auto-finds a child particle system
    }

    private void FixedUpdate() // stable to read rigidbody velocity
    {
        if (particles == null) return; // no particles assigned

        float speed = rb.linearVelocity.magnitude; // current ball speed from rigidbody
        float t = Mathf.InverseLerp(0f, speedAtMaxEmission, speed); // normalizing speed into 0-1 range -> InverseLerp(a,b,value) = (value-a)/(b-a)
        float rate = Mathf.Lerp(minEmissionRate, maxEmissionRate, t); // linear mapping from speed to emission rate -> Lerp(a,b,t) = a+(b-a)*t

        var emission = particles.emission; // getting emission module
        emission.rateOverTime = rate; // setting emission rate over time to computed rate

        var main = particles.main; // getting main module for start speed control
        float startSpeed = Mathf.Lerp(minParticleStartSpeed, maxParticleStartSpeed, t); // linear mapping for particle start speed
        main.startSpeed = startSpeed; // applying start speed to particles

        if (rate <= 0.01f) // effectively not emitting
        {
            if (particles.isPlaying) particles.Stop(true, ParticleSystemStopBehavior.StopEmitting); // stops emitting but keeps existing particles
        }
        else // should be emitting
        {
            if (!particles.isPlaying) particles.Play(true); // ensures particle system is playing
        }
    }
}
