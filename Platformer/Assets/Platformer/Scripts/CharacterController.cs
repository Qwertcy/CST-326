using UnityEngine;

public class CharacterController : MonoBehaviour
{
    Rigidbody rb;
    private Animator animator;
    private Collider col; //ground detection

    public float acceleration = 30f;
    public float maxSpeed = 10f;
    public float jumpImpulse = 7f;
    public float jumpBoostForce = 20f;
    public float maxJumpHoldTime = 0.15f;

    private float jumpHoldTimer = 0f; // tracks remaining boost time after jump
    private bool inAir;
    public bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();

        if (animator == null)
            Debug.LogError("Animator component is missing from " + gameObject.name);

        if (rb == null)
            Debug.LogError("Rigidbody component is missing from " + gameObject.name);

        if (col == null)
            Debug.LogError("Collider component is missing from " + gameObject.name);
    }

    void Update()
    {
        float horizontalAmount = Input.GetAxis("Horizontal"); // reads horizontal input (-1 to 1)

        // horizontal movement
        rb.linearVelocity += Vector3.right * horizontalAmount * acceleration * Time.deltaTime; // accelerates player left/right

        float horizontalSpeed = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed); // clamps horizontal velocity to max speed
        rb.linearVelocity = new Vector3(horizontalSpeed, rb.linearVelocity.y, rb.linearVelocity.z); // applies clamped horizontal speed

        if (animator != null)
            animator.SetFloat("Speed", Mathf.Abs(horizontalAmount)); // updates movement animation parameter

        // ground check using raycast
        float castDistance = col.bounds.extents.y + 0.05f; // slightly below collider bottom
        Vector3 startPoint = transform.position;
        isGrounded = Physics.Raycast(startPoint, Vector3.down, castDistance); // checks if ground is below

        Debug.DrawLine(startPoint, startPoint + castDistance * Vector3.down, isGrounded ? Color.green : Color.red); // visual debug line

        
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            jumpHoldTimer = maxJumpHoldTime; // resets boost timer
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpImpulse, rb.linearVelocity.z); // applies initial upward velocity
            inAir = true;

            if (animator != null)
                animator.SetBool("IsJumping", true);
        }

        // jump hold
        if (Input.GetKey(KeyCode.Space) && jumpHoldTimer > 0f)
        {
            rb.AddForce(Vector3.up * jumpBoostForce, ForceMode.Acceleration); // adds upward acceleration while held
            jumpHoldTimer -= Time.deltaTime;
        }
        else
        {
            jumpHoldTimer = 0f;
        }

        // landing detection
        if (isGrounded && inAir)
        {
            inAir = false;

            if (animator != null)
                animator.SetBool("IsJumping", false);
        }

        // smooth deceleration when stopping
        if (horizontalAmount == 0)
        {
            Vector3 decayVelocity = rb.linearVelocity;
            decayVelocity.x *= 1f - Time.deltaTime * 4f; // gradually reduces horizontal speed
            rb.linearVelocity = decayVelocity;
        }
        else
        {
            float yawRotation = (horizontalAmount > 0) ? 90f : -90f; // determines facing direction
            transform.rotation = Quaternion.Euler(0f, yawRotation, 0f); // rotates character to face movement direction
        }
    }
}


