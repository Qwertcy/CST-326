using UnityEngine;

public class PowerupSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject[] powerupPrefabs; // list of powerup prefabs

    [Header("Spawn Timing")]
    [SerializeField] private float minSpawnDelay = 4f;
    [SerializeField] private float maxSpawnDelay = 8f;

    [Header("Spawn Area")]
    [SerializeField] private Vector3 center = Vector3.zero; // center of spawn box
    [SerializeField] private Vector3 size = new Vector3(10f, 0f, 5f); // size of spawn box

    [Header("Limits")]
    [SerializeField] private int maxAlivePowerups = 1;

    private float nextSpawnAt;
    private int aliveCount;

    private void Start()
    {
        ScheduleNext();
    }

    private void Update()
    {
        if (powerupPrefabs == null || powerupPrefabs.Length == 0) return; // no prefabs configured
        if (aliveCount >= maxAlivePowerups) return;
        if (Time.time < nextSpawnAt) return;

        SpawnOne();
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
        nextSpawnAt = Time.time + delay; // converts delay to absolute timestamp
    }

    private void SpawnOne()
    {
        int idx = Random.Range(0, powerupPrefabs.Length); // choosing which prefab to spawn
        GameObject prefab = powerupPrefabs[idx]; // getting prefab reference
        if (prefab == null) return; // safety guard for empty slot

        Vector3 half = size * 0.5f; // half extents for random point selection
        float x = Random.Range(center.x - half.x, center.x + half.x); // random x within bounds
        float y = center.y; // keeping y fixed for pong plane
        float z = Random.Range(center.z - half.z, center.z + half.z); // random z within bounds
        Vector3 pos = new Vector3(x, y, z); // composing spawn position

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity); // spawning prefab at computed position
        aliveCount++;

        PowerupPickup pickup = obj.GetComponent<PowerupPickup>(); // getting pickup script to notify on destroy
        if (pickup != null) pickup.SetSpawner(this); // spawner reference for decrementing alive count
    }

    public void NotifyPowerupConsumed()
    {
        aliveCount = Mathf.Max(0, aliveCount - 1); // decrements alive count safely (cannot go negative)
    }

    private void OnDrawGizmosSelected() // editor-only visualization of spawn bounds
    {
        Gizmos.matrix = Matrix4x4.identity; // reset gizmo matrix to world
        Gizmos.DrawWireCube(center, size); // draw spawn box in scene view
    }
}
