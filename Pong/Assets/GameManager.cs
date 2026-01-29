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
        if (isLeftGoal) rightPlayerScore++;
        else leftPlayerScore++;

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

//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using UnityEngine.SceneManagement;

//public class GameManager : MonoBehaviour
//{
//    public static GameManager Instance;

//    public int leftPlayerScore = 0;
//    public int rightPlayerScore = 0;

//    private TextMeshProUGUI leftScoreText;
//    private TextMeshProUGUI rightScoreText;

//    [Header("Audio Settings")]
//    public AudioClip goalClip;
//    private AudioSource audioSource;

//    private void Awake()
//    {
//        if (Instance == null)
//            Instance = this;
//        else
//            Destroy(gameObject);

//        leftScoreText = GameObject.FindGameObjectWithTag("LeftScore").GetComponent<TextMeshProUGUI>();
//        rightScoreText = GameObject.FindGameObjectWithTag("RightScore").GetComponent<TextMeshProUGUI>();

//        audioSource = GetComponent<AudioSource>();
//        if (audioSource == null)
//        {
//            audioSource = gameObject.AddComponent<AudioSource>();
//        }
//    }

//    public void GoalScored(bool isLeftGoal)
//    {
//        if (isLeftGoal)
//        {
//            rightPlayerScore++;
//            Debug.Log("Right player scores! Score: "
//                      + leftPlayerScore + " - " + rightPlayerScore);
//        }
//        else
//        {
//            leftPlayerScore++;
//            Debug.Log("Left player scores! Score: "
//                      + leftPlayerScore + " - " + rightPlayerScore);
//        }

//        PlayGoalSound();

//        UpdateScoreTexts();

//        // Invoke(nameof(ResetBall), goalClip.length);
//        ResetBall();
//    }

//    private void ResetBall()
//    {
//        Ball ball = Object.FindAnyObjectByType<Ball>();
//        if (ball != null)
//        {
//            ball.ResetPositionAndLaunch();
//        }
//    }

//    private void UpdateScoreTexts()
//    {
//        leftScoreText.text = leftPlayerScore.ToString();
//        rightScoreText.text = rightPlayerScore.ToString();
//    }

//    private void PlayGoalSound()
//    {
//        if (goalClip == null || audioSource == null)
//        {
//            Debug.LogWarning("Goal sound clip or AudioSource is missing!");
//            return;
//        }

//        audioSource.PlayOneShot(goalClip);
//    }

//}
