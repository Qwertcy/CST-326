using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [SerializeField] private bool isLeftGoal = false; // true if trigger is the left goal (right player scores)

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ball")) return; // only respond to the ball
        if (GameManager.Instance == null) return; // safety check
        GameManager.Instance.GoalScored(isLeftGoal); // notify gamemanager which side was scored on
    }
}
