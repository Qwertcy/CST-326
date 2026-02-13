using UnityEngine;

public class PowerupPickup : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float despawnSeconds = 8f; // auto-destroys if not picked up

    [Header("Effects")]
    [SerializeField] private float speedMultiplierMin = 0.7f;
    [SerializeField] private float speedMultiplierMax = 1.5f;
    [SerializeField] private float trajectoryAngleMinDeg = -25f;
    [SerializeField] private float trajectoryAngleMaxDeg = 25f;

    private PowerupSpawner spawner; // back-reference to notify spawner on consume

    private void Start()
    {
        Invoke(nameof(Despawn), despawnSeconds); // auto-destroys after some time if uncollected
    }

    public void SetSpawner(PowerupSpawner owningSpawner) // allows spawner to be notified when we go away
    {
        spawner = owningSpawner; // store spawner reference
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return;
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        ApplyRandomEffect(rb);
        Consume();
    }

    private void ApplyRandomEffect(Rigidbody ballRb)
    {
        int choice = Random.Range(0, 2); // 0 = speed change, 1 = trajectory change

        if (choice == 0)
        {
            float mult = Random.Range(speedMultiplierMin, speedMultiplierMax);
            Vector3 v = ballRb.linearVelocity;
            float speed = v.magnitude;
            if (speed <= 0.0001f) return; // avoids normalizing near-zero vectors

            Vector3 dir = v.normalized; // direction of travel
            float newSpeed = speed * mult;
            ballRb.linearVelocity = dir * newSpeed; // writes back new velocity with same direction
        }
        else
        {
            Vector3 v = ballRb.linearVelocity;
            float speed = v.magnitude;
            if (speed <= 0.0001f) return; // guards against zero velocity

            Vector3 dir = v.normalized; // normalizes to direction
            float angleDeg = Random.Range(trajectoryAngleMinDeg, trajectoryAngleMaxDeg); // chooses a random rotation angle
            Quaternion rot = Quaternion.Euler(0f, angleDeg, 0f); // rotates around y to change x-z direction
            Vector3 newDir = (rot * dir).normalized; // rotates direction and renormalizes
            ballRb.linearVelocity = newDir * speed; // keeps speed the same but changes heading
        }
    }

    private void Consume() // handles cleanup when collected
    {
        CancelInvoke(nameof(Despawn)); // stops despawn timer because we're consuming (otherwise it double decrements alive count)
        if (spawner != null) spawner.NotifyPowerupConsumed(); // notifies spawner so it can allow future spawns
        Destroy(gameObject); // removes pickup from scene
    }

    private void Despawn() // called when lifetime expires
    {
        if (spawner != null) spawner.NotifyPowerupConsumed();
        Destroy(gameObject); // destroys after timeout
    }
}
