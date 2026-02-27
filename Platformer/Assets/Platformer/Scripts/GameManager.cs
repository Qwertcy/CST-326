using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI scoreText;

    [Header("Game Rules")]
    public int levelDurationSeconds = 100;
    public bool stopTimeOnEnd = true;

    private float levelStartTime;
    private bool gameEnded; // prevents win/lose from triggering multiple times

    public int coinCount = 0;
    public int score = 0;

    void Start()
    {
        levelStartTime = Time.time; // records start time so timer counts down from 100
        gameEnded = false;

        UpdateUI(); // ensures ui is correct at time 0

        if (Camera.main == null)
        {
            Debug.LogError("No Main Camera found");
        }
    }

    void Update()
    {
        if (gameEnded) return;

        int timeLeft = GetTimeLeftSeconds();

        if (timerText != null) timerText.text = timeLeft.ToString(); // displays time remaining as an integer

        if (timeLeft <= 0)
        {
            FailLevel("Time ran out!");
        }
    }

    int GetTimeLeftSeconds() // helper to compute remaining seconds
    {
        float elapsed = Time.time - levelStartTime; // how many seconds have passed since start
        int timeLeft = levelDurationSeconds - (int)elapsed; // converts elapsed to int seconds and subtract
        return Mathf.Max(0, timeLeft); // clamp to 0 so it never shows negative time
    }

    void UpdateUI() // updates all ui elements from the current game state
    {
        if (coinsText != null) coinsText.text = $": {coinCount}";
        if (scoreText != null) scoreText.text = $"Score: {score.ToString()}";
        if (timerText != null) timerText.text = $"Time left: {GetTimeLeftSeconds().ToString()}";
    }

    public void AddScore(int amount)
    {
        if (gameEnded) return;
        score += amount;
        UpdateUI();
    }

    public void AddCoin(int amount)
    {
        if (gameEnded) return;
        coinCount += amount;
        UpdateUI();
    }

    public void CompleteLevel()
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log("Level complete!");


        if (stopTimeOnEnd) Time.timeScale = 0f; // freezes physics/updates for a clean stop
    }

    public void FailLevel(string reason)
    {
        if (gameEnded) return;
        gameEnded = true;

        Debug.Log($"Level failed: {reason}");

        if (stopTimeOnEnd) Time.timeScale = 0f; // freezes the game so failure is visible
    }
//    public void EndGame()
//    {
//        Debug.Log("Game Over - Player fell into a pit!");

////stops play mode
//#if UNITY_EDITOR
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//    Application.Quit();
//#endif
//    }

}
