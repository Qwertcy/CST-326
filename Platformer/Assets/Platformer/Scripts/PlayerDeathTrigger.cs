using UnityEngine;

public class PlayerDeathTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null) Debug.LogError("No GameManager found");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Death")) return;

        Debug.Log("Player hit a death trigger!");

        if (gameManager != null) gameManager.FailLevel("fell");
    }
}