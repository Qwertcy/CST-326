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
        if (!collision.collider.CompareTag("Rock") && !collision.collider.CompareTag("Question")) return; // ignores non-block collisions
        if (rb.linearVelocity.y <= 0f) return; // must be moving upward to count as a hit

        bool hitFromBelow = false;

        for (int i = 0; i < collision.contactCount; i++) // loops through all contact points in collision
        {
            Vector3 n = collision.GetContact(i).normal; // gets the surface normal at this contact
            if (n.y < -0.5f) // negative y means the normal points downward
            {
                hitFromBelow = true;
                break;
            }
        }

        if (!hitFromBelow) return;

        if (collision.collider.CompareTag("Rock"))
        {
            Destroy(collision.collider.gameObject);
        }
        else if (collision.collider.CompareTag("Question"))
        {
            if (gameManager != null) gameManager.AddCoin(); // awards a coin when hit from below
        }
    }
}

//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class PlayerBlockHit : MonoBehaviour
//{
//    private Rigidbody rb;
//    private GameManager gameManager;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        gameManager = FindObjectOfType<GameManager>();
//        if (gameManager == null)
//        {
//            Debug.LogError("No GameManager found in the scene. Make sure one exists.");
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {

//        if (collision.collider.CompareTag("Rock") || collision.collider.CompareTag("Question"))
//        {
//            if (rb.linearVelocity.y > 0f)
//            {
//                if (collision.collider.CompareTag("Rock"))
//                {

//                    Destroy(collision.collider.gameObject);
//                }

//                else if (collision.collider.CompareTag("Question"))
//                {
//                    if (gameManager != null)
//                    {
//                        gameManager.AddCoin();
//                    }
//                }
//            }
//        }
//    }
//}
