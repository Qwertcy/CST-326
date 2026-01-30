using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("tuning")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private int winCount = 12;

    [Header("ui")]
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI statusText; // ui text for win/lose message
    [SerializeField] private GameObject statusRoot; // root object to show/hide status ui

    [Header("references")]
    [SerializeField] private GameObject enemy;

    private Rigidbody rb;
    private int count;
    private Vector2 moveInput;
    private bool gameOver; // prevents logic from running after win/lose

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        count = 0;
        UpdateCountUI();

        if (statusRoot != null) statusRoot.SetActive(false); // hides status ui at game start
    }

    private void OnMove(InputValue movementValue) // input system callback for movement
    {
        if (gameOver) return; // ignore inputs after game ends

        moveInput = movementValue.Get<Vector2>(); // reads stick/wasd input as a 2d vector
    }

    private void FixedUpdate()
    {
        if (gameOver) return;

        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y); // converts 2d input into 3d world movement
        rb.AddForce(movement * speed); // applies force for rolling motion
    }

    private void OnTriggerEnter(Collider other) // trigger for pickups
    {
        if (gameOver) return; // ignore pickups after game ends

        if (other.CompareTag("Pickup"))
        {
            other.gameObject.SetActive(false); // disables pickup instead of destroying (cheap and simple)
            count++;
            UpdateCountUI();

            if (count >= winCount)
            {
                WinGame();
            }
        }
    }

    private void OnCollisionEnter(Collision collision) // collision for enemy hit (lose condition)
    {
        if (gameOver) return; // prevents multiple triggers

        if (collision.gameObject.CompareTag("Enemy"))
        {
            LoseGame(collision.gameObject);
        }
    }

    private void UpdateCountUI()
    {
        if (countText != null) countText.text = $"Count: {count}";
    }

    private void WinGame() // handles win: show message and remove enemy
    {
        gameOver = true; // freezes further input/physics and triggers

        ShowStatus("You win!");

        moveInput = Vector2.zero; // clears input so we stop pushing
        rb.linearVelocity = Vector3.zero; // stops motion immediately
        rb.angularVelocity = Vector3.zero; // stops rolling spin

        if (enemy != null) enemy.SetActive(false); // disables enemy without destroying it (prevents missing reference errors)
    }

    private void LoseGame(GameObject collidedEnemy) // handles loss: freeze player and make enemy celebrate
    {
        gameOver = true; // 1) ends the run so input/physics logic stops in other methods

        ShowStatus("You lose!"); // 2) shows the lose ui immediately for feedback

        moveInput = Vector2.zero; // 3) clears input so we don't apply any more force
        rb.linearVelocity = Vector3.zero; // 4) stops linear motion instantly (use rb.linearVelocity if your unity supports it)
        rb.angularVelocity = Vector3.zero; // 5) stops spinning instantly
        rb.isKinematic = true; // 6) freezes the rigidbody so physics can't move the player anymore

        GameObject enemyObject = enemy != null ? enemy : collidedEnemy; // 7) chooses a single enemy reference to operate on
        if (enemyObject == null) return; // 8) exits early if we somehow have no enemy

        EnemyMovement movement = enemyObject.GetComponent<EnemyMovement>(); // 9) gets the enemy chase script so we can stop it
        if (movement != null) movement.enabled = false; // 10) disables chase so enemy stays at the catch position

        EnemyFaceController face = enemyObject.GetComponent<EnemyFaceController>(); // 11) gets face controller so we can force expression
        if (face != null) face.LockToGrin(); // 12) locks grin so distance logic can't switch back to angry

        EnemyVictoryJump victory = enemyObject.GetComponent<EnemyVictoryJump>(); // 13) gets the victory bounce component
        if (victory != null) victory.StartCelebration(); // 14) starts bounce last so it captures the current (caught) position
    }


    private void ShowStatus(string message)
    {
        if (statusRoot != null) statusRoot.SetActive(true);
        if (statusText != null) statusText.text = message;
    }
}

//using System.Collections.Specialized;
//using UnityEngine;
//using UnityEngine.InputSystem;
//using TMPro;

//public class PlayerController : MonoBehaviour
//{
//    public float speed = 0;
//    public TextMeshProUGUI countText;
//    public GameObject winTextObject;

//    private Rigidbody rb;
//    private int count;
//    private float movementX;
//    private float movementY;
//    // Start is called once before the first execution of Update after the MonoBehaviour is created
//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        count = 0;
//        SetCountText();
//        winTextObject.SetActive(false);
//    }

//    void OnMove(InputValue movementValue)
//    {
//        Vector2 movementVector = movementValue.Get<Vector2>();

//        movementX = movementVector.x;
//        movementY = movementVector.y;
//    }

//    void SetCountText()
//    {
//        countText.text = "Count: " + count.ToString();
//        if (count >= 12)
//        {
//            winTextObject.SetActive(true);
//            Destroy(GameObject.FindGameObjectWithTag("Enemy"));
//        }
//    }

//    void FixedUpdate()
//    {
//        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
//        rb.AddForce(movement * speed);
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        if (collision.gameObject.CompareTag("Enemy"))
//        {
//            Destroy(gameObject);
//            winTextObject.SetActive(true);
//            winTextObject.GetComponent<TextMeshProUGUI>().text = "You lose!";
//        }
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.gameObject.CompareTag("Pickup"))
//        {
//            other.gameObject.SetActive(false);
//            count++;

//            SetCountText();
//        }
//    }
//}
