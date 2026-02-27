using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {

        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddCoin(1);
            gameManager.AddScore(100);
        }

        Destroy(gameObject); // removes the coin so it can't be collected twice
    }
}