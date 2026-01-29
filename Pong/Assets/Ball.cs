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

            rb.linearVelocity = newDir.normalized * currentSpeed; // apply new direction and new target speed
        }
        else if (other.gameObject.CompareTag("Wall")) // wall collision: physics handles reflection; we only play sfx
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

        float dir = (Random.value > 0.5f) ? 1f : -1f; // choose random left/right direction
        rb.linearVelocity = new Vector3(dir, 0f, 0f) * currentSpeed; // relaunch at currentSpeed (consistent with gameplay settings)
    }

    private void UpdateBallColor() // changes color from yellow->red as speed increases
    {
        if (ballRenderer == null || ballRenderer.material == null) return; // guard for missing renderer/material
        float speed = rb.linearVelocity.magnitude; // compute current speed
        float t = Mathf.InverseLerp(initialSpeed, maxSpeed, speed); // map speed into 0..1 range
        ballRenderer.material.color = Color.Lerp(Color.yellow, Color.red, t); // lerp color based on t
    }
}


//using UnityEngine;
//using UnityEngine.Audio;
//using UnityEngine.InputSystem;

//public class Ball : MonoBehaviour
//{
//    [Header("Speed Settings")]
//    public float maxSpeed = 20f;
//    public float initialSpeed = 2.0f;
//    public float speedMultiplier = 1.2f;
//    private float currentSpeed;

//    [Header("Paddle Bounce Angle Settings")]
//    [Tooltip("Maximum angle (in degrees) away from horizontal for paddle bounces.")]
//    public float maxBounceAngleDeg = 45f;

//    [Header("Wall Bounce Settings")]
//    [Tooltip("Angle (in degrees) to use for wall bounces.")]
//    public float wallBounceAngleDeg = 45f;

//    [Header("Audio Clips")]
//    public AudioClip paddleBounceClip;
//    public AudioClip wallBounceClip;
//    public AudioClip goalClip;
//    [Tooltip("Played on collisions if current speed >= highSpeedThreshold.")]
//    public AudioClip highSpeedClip;
//    public float highSpeedThreshold = 8f;

//    private Rigidbody rb;
//    private Renderer ballRenderer;
//    private AudioSource audioSource;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();

//        ballRenderer = GetComponent<Renderer>();

//        audioSource = GetComponent<AudioSource>();
//        if (audioSource == null)
//        {
//            Debug.LogError("No AudioSource found on the Ball! Adding one now.");
//            audioSource = gameObject.AddComponent<AudioSource>();
//        }


//        currentSpeed = initialSpeed;
//        Vector3 startVelocity = new Vector3(1f, 0f, 0f).normalized * currentSpeed; //starting ball moving in positive x
//        rb.linearVelocity = startVelocity;
//    }

//    void Update()
//    {
//        UpdateBallColor();
//    }

//    private void OnCollisionEnter(Collision other)
//    {
//        float speed = rb.linearVelocity.magnitude;

//        if (other.gameObject.CompareTag("Paddle"))
//        {
//            currentSpeed = Mathf.Min(currentSpeed * speedMultiplier, maxSpeed);

//            if (speed >= highSpeedThreshold && highSpeedClip != null)
//            {
//                audioSource.clip = highSpeedClip;
//            }
//            else
//            {
//                audioSource.clip = paddleBounceClip;
//            }
//            audioSource.Play();


//            ContactPoint contact = other.contacts[0];
//            float paddleCenterZ = other.collider.bounds.center.z;
//            float paddleExtentZ = other.collider.bounds.extents.z;
//            float hitFactor = (contact.point.z - paddleCenterZ) / paddleExtentZ;
//            float bounceAngle = hitFactor * maxBounceAngleDeg * Mathf.Deg2Rad;
//            float directionX = (other.transform.position.x < transform.position.x) ? 1f : -1f;
//            Vector3 newDirection = new Vector3(directionX * Mathf.Cos(bounceAngle), 0f, Mathf.Sin(bounceAngle));
//            rb.linearVelocity = newDirection.normalized * currentSpeed;
//        }
//        else if (other.gameObject.CompareTag("Wall"))
//        {
//            if (speed >= highSpeedThreshold && highSpeedClip != null)
//            {
//                audioSource.clip = highSpeedClip;
//            }
//            else
//            {
//                audioSource.clip = wallBounceClip;
//            }
//            audioSource.PlayOneShot(wallBounceClip);
//        }

//            Debug.Log($"currentSpeed: {currentSpeed}");
//    }
//    public void ResetPositionAndLaunch()
//    {
//        transform.position = Vector3.zero; //moving the ball to the centre

//        Rigidbody rb = GetComponent<Rigidbody>();
//        rb.linearVelocity = Vector3.zero;
//        rb.angularVelocity = Vector3.zero; //zeroing out velocity
//        currentSpeed = 4f;

//        float startXDirection = (Random.value > 0.5f) ? 1f : -1f; //currently sending in random direction
//        Vector3 startVelocity = new Vector3(startXDirection, 0f, 0f).normalized * 2f;
//        rb.linearVelocity = startVelocity;
//    }

//    private void UpdateBallColor()
//    {
//        if (ballRenderer == null || ballRenderer.material == null) return;

//        float speed = rb.linearVelocity.magnitude;

//        // 0 = yellow, 1 = red
//        float t = Mathf.InverseLerp(initialSpeed, maxSpeed, speed);

//        // interpolate from yellow to red
//        Color newColor = Color.Lerp(Color.yellow, Color.red, t);

//        ballRenderer.material.color = newColor;
//    }

//}

