using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerBlockHit : MonoBehaviour
{
    private Rigidbody rb;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindFirstObjectByType<GameManager>();

        if (gameManager == null)
            Debug.LogError("No GameManager");
    }

    private void OnCollisionEnter(Collision collision)
    {
        bool isRock = collision.collider.CompareTag("Rock");
        bool isQuestion = collision.collider.CompareTag("Question");

        if (!isRock && !isQuestion) return; // ignores collisions that aren't relevant blocks
        if (rb.linearVelocity.y <= 0f) return; // only counts hits while moving upward (jumping)

        bool hitFromBelow = false;

        for (int i = 0; i < collision.contactCount; i++) // iterates all contact points in this collision
        {
            Vector3 n = collision.GetContact(i).normal; // reads the surface normal at the contact
            if (n.y < -0.5f) // downward normal means we contacted the underside of the block
            {
                hitFromBelow = true;
                break;
            }
        }

        if (!hitFromBelow) return;

        if (isRock)
        {
            if (gameManager != null) gameManager.AddScore(100);
            Destroy(collision.collider.gameObject);
        }
    }
}
