using UnityEngine;

public class EnemyVictoryJump : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 0.5f;
    [SerializeField] private float jumpSpeed = 4f;

    private Vector3 basePosition; // position we bounce around (captured at win time)
    private bool celebrating; // whether celebration is active

    public void StartCelebration() // called when the enemy catches the player
    {
        if (celebrating) return; // prevents restarting celebration
        celebrating = true;
        basePosition = transform.position; // captures the exact catch position so there is no snap-back
    }

    private void Update()
    {
        if (!celebrating) return;

        float yOffset = Mathf.Abs(Mathf.Sin(Time.time * jumpSpeed)) * jumpHeight; // creates repeating positive bounce offset
        transform.position = basePosition + Vector3.up * yOffset; // applies bounce relative to captured position
    }
}
