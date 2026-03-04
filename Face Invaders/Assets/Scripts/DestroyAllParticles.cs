using UnityEngine;

public class DestroyAllParticles : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (ps && !ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
