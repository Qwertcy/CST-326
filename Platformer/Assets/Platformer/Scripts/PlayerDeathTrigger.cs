using UnityEngine;

public class PlayerDeathTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("No GameManager found in the scene. Please add one.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death"))
        {
            Debug.Log("Player fell into a pit!");

            if (gameManager != null)
            {
                gameManager.EndGame();
            }
        }
    }
}
