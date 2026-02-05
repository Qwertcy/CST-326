using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour // manages scoring, ui updates, and goal flow
{
    public static GameManager Instance; // singleton-like global reference for easy access

    [Header("Score State")]
    [SerializeField] private int leftPlayerScore = 0;
    [SerializeField] private int rightPlayerScore = 0;

    [Header("UI References (assign in inspector)")]
    [SerializeField] private TextMeshProUGUI leftScoreText;
    [SerializeField] private TextMeshProUGUI rightScoreText;

    [Header("Goal Flow")]
    [SerializeField] private float resetDelaySeconds = 0.35f; // delay so goal sound isn't masked by immediate new collisions

    [Header("Audio Settings")]
    [SerializeField] private AudioClip goalClip;
    [SerializeField] private float goalVolume = 1.0f;

    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // force 2d audio
        audioSource.dopplerLevel = 0f; // disables doppler pitch artifacts
    }

    private void Start()
    {
        UpdateScoreTexts();
    }

    public void GoalScored(bool isLeftGoal)
    {
        if (isLeftGoal)
        {
            rightPlayerScore++; 
            Debug.Log("goal scored on LEFT player"); 
        }
        else
        {
            leftPlayerScore++;
            Debug.Log("goal scored on RIGHT player");
        }

        UpdateScoreTexts();
        PlayGoalSound();
        CancelInvoke(nameof(ResetBall)); // prevents multiple overlapping resets if ball re-triggers quickly
        Invoke(nameof(ResetBall), resetDelaySeconds); // resets after a short delay to avoid masking the goal sound
    }

    private void ResetBall()
    {
        Ball ball = Object.FindAnyObjectByType<Ball>();
        if (ball == null) return;
        ball.ResetPositionAndLaunch();
    }

    private void UpdateScoreTexts()
    {
        if (leftScoreText != null) leftScoreText.text = leftPlayerScore.ToString();
        if (rightScoreText != null) rightScoreText.text = rightPlayerScore.ToString();
    }

    private void PlayGoalSound()
    {
        if (goalClip == null) return;
        if (audioSource == null) return;
        audioSource.PlayOneShot(goalClip, goalVolume);
    }
}
