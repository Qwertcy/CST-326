using UnityEngine;

public class EnemyVictoryJump : MonoBehaviour // bounces in place starting from where victory begins
{
    [SerializeField] private float jumpHeight = 0.5f; // how high to bounce
    [SerializeField] private float jumpSpeed = 4f; // how fast to bounce

    private Vector3 basePosition; // position we bounce around (captured at win time)
    private bool celebrating; // whether celebration is active

    public void StartCelebration() // called when the enemy catches the player
    {
        if (celebrating) return; // prevents restarting celebration
        celebrating = true; // enables bounce update loop
        basePosition = transform.position; // captures the exact catch position so there is no snap-back
    }

    private void Update() // runs each frame for smooth animation
    {
        if (!celebrating) return; // no work unless celebrating

        float yOffset = Mathf.Abs(Mathf.Sin(Time.time * jumpSpeed)) * jumpHeight; // creates repeating positive bounce offset
        transform.position = basePosition + Vector3.up * yOffset; // applies bounce relative to captured position
    }
}
