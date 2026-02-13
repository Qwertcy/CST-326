using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Speed Settings")]
    public float maxSpeed = 25f;
    public float initialSpeed = 2.0f;
    public float speedMultiplier = 1.2f; // speed scaling applied on paddle hits
    private float currentSpeed; // runtime speed target used when we overwrite velocity

    [Header("Paddle Bounce Angle Settings")]
    [Tooltip("Maximum angle (in degrees) away from horizontal for paddle bounces.")]
    public float maxBounceAngleDeg = 45f; // max deflection to avoid near-vertical or boring trajectories

    [Header("Audio Clips")]
    public AudioClip paddleBounceClip;
    public AudioClip wallBounceClip;
    public AudioClip highSpeedClip;
    public float highSpeedThreshold = 10f;
    [Range(0f, 1f)] public float bounceVolume = 1.0f; // volume scaling for bounce impacts

    private Rigidbody rb; // cached rigidbody for velocity reads/writes
    private Renderer ballRenderer; // cached renderer for speed-to-color mapping
    private AudioSource sfxSource; // dedicated audio source for ball sfx

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        ballRenderer = GetComponent<Renderer>();

        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2d audio reduces weirdness and keeps sfx consistent regardless of position
        sfxSource.dopplerLevel = 0f; // removes doppler pitch changes that can feel like "delay" or "warble"

        currentSpeed = initialSpeed;
        rb.linearVelocity = Vector3.right * currentSpeed; // launch to the right at initial speed
    }

    private void Update()
    {
        UpdateBallColor(); // keep color synced with speed
    }

    private void OnCollisionEnter(Collision other)
    {
        float speedNow = rb.linearVelocity.magnitude; // capture current speed before we modify velocity

        if (other.gameObject.CompareTag("Paddle")) // paddle collision: accelerate + apply angle control + play sfx
        {
            currentSpeed = Mathf.Min(currentSpeed * speedMultiplier, maxSpeed); // scale up speed, clamped to max
            PlayImpactSound(speedNow, paddleBounceClip); // play appropriate clip (normal vs high-speed) using oneshot

            ContactPoint contact = other.contacts[0]; // use first contact point for hit location calculation
            float paddleCenterZ = other.collider.bounds.center.z; // world z center of paddle bounds
            float paddleExtentZ = other.collider.bounds.extents.z; // half-size in z for normalization
            float hitFactor = (contact.point.z - paddleCenterZ) / paddleExtentZ; // roughly -1..+1 based on where we hit
            hitFactor = Mathf.Clamp(hitFactor, -1f, 1f); // clamp to avoid extreme angles if contact is slightly outside bounds

            float bounceAngleRad = hitFactor * maxBounceAngleDeg * Mathf.Deg2Rad; // convert hit location to bounce angle in radians

            float directionX = (other.transform.position.x < transform.position.x) ? 1f : -1f; // ensure ball goes away from paddle

            Vector3 newDir = new Vector3( // build direction from angle components
                directionX * Mathf.Cos(bounceAngleRad), // x stays dominant unless extreme hit
                0f, // lock to x-z plane
                Mathf.Sin(bounceAngleRad) // z varies based on hit position
            );

            rb.linearVelocity = newDir.normalized * currentSpeed; // new direction and new target speed
        }
        else if (other.gameObject.CompareTag("Wall")) // wall collision: physics handles reflection; only play sfx
        {
            PlayImpactSound(speedNow, wallBounceClip); // choose normal vs high-speed and play immediately
        }
    }

    private void PlayImpactSound(float speedNow, AudioClip normalClip) // centralizes clip selection logic for collisions
    {
        if (sfxSource == null) return;
        AudioClip chosen = normalClip; // default to normal clip
        if (speedNow >= highSpeedThreshold && highSpeedClip != null) chosen = highSpeedClip; // switch to high-speed clip if threshold met
        if (chosen == null) return; // if nothing assigned, do nothing (avoids warnings)
        sfxSource.PlayOneShot(chosen, bounceVolume); // oneshot avoids clip swapping and supports rapid successive impacts cleanly
    }

    public void ResetPositionAndLaunch() // called by gamemanager after a goal
    {
        transform.position = Vector3.zero; // reset to center

        rb.linearVelocity = Vector3.zero; // stop movement so next launch is consistent
        rb.angularVelocity = Vector3.zero; // stop spin as well

        currentSpeed = initialSpeed; // reset target speed to starting speed so rallies restart fairly

        float dir = (Random.value > 0.5f) ? 1f : -1f; // random left/right direction
        rb.linearVelocity = new Vector3(dir, 0f, 0f) * currentSpeed; // relaunch at currentSpeed
    }

    private void UpdateBallColor() // changes color from yellow->red as speed increases
    {
        if (ballRenderer == null || ballRenderer.material == null) return; // guard for missing renderer/material
        float speed = rb.linearVelocity.magnitude; // compute current speed
        float t = Mathf.InverseLerp(initialSpeed, maxSpeed, speed); // map speed into 0..1 range
        ballRenderer.material.color = Color.Lerp(Color.yellow, Color.red, t); // lerp color based on t
    }
}

