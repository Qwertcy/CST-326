using UnityEngine;

public class EnemyFaceController : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private SpriteRenderer faceRenderer;

    [Header("faces")]
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private Sprite grinSprite;

    [Header("distance thresholds (hysteresis)")]
    [SerializeField] private float grinEnterDistance = 4f; // distance at/below which we enter grinning state
    [SerializeField] private float grinExitDistance = 5f; // distance at/above which we exit grinning state

    private bool isGrinning; // cached state so we don't flip sprites every frame near the boundary

    private void Awake()
    {
        if (grinExitDistance < grinEnterDistance) // ensures hysteresis is valid (exit must be farther than enter)
        {
            grinExitDistance = grinEnterDistance; // clamps to avoid impossible state behavior if misconfigured in inspector
        }
    }

    private void Start()
    {
        UpdateState(forceApply: true); // initializes sprite immediately so it matches the starting distance
    }

    private void Update()
    {
        UpdateState(forceApply: false); // evaluates state transitions and applies sprite only on change
    }

    private void UpdateState(bool forceApply) // centralizes state logic so Start/Update share the same behavior
    {
        if (playerTransform == null || faceRenderer == null) return; // prevents null reference errors if not wired

        if (isLocked) return; // prevents flipping when we intentionally lock expression

        float distance = Vector3.Distance(transform.position, playerTransform.position); // computes world-space distance

        bool nextIsGrinning = isGrinning; // default to current state; we only change it when a threshold is crossed

        if (!isGrinning && distance <= grinEnterDistance) // if currently angry and we got close enough, enter grin
        {
            nextIsGrinning = true;
        }
        else if (isGrinning && distance >= grinExitDistance) // if currently grinning and we moved far enough away, exit
        {
            nextIsGrinning = false;
        }

        if (forceApply || nextIsGrinning != isGrinning) // only update visuals when state changes or on forced init
        {
            isGrinning = nextIsGrinning; // commits the new state so it persists across frames
            faceRenderer.sprite = isGrinning ? grinSprite : angrySprite; // sets sprite corresponding to the committed state
        }
    }

    private bool isLocked; // when true, distance logic stops changing the face

    public void LockToGrin() // forces grin and prevents further automatic switching
    {
        isLocked = true; // disables distance-based switching
        isGrinning = true; // sets internal state so it stays consistent
        if (faceRenderer != null) faceRenderer.sprite = grinSprite; // applies grin sprite immediately
    }


    private void OnValidate() // runs in editor when values change; helps catch bad thresholds early
    {
        if (grinEnterDistance < 0f) grinEnterDistance = 0f; // prevents negative distances which don't make sense
        if (grinExitDistance < grinEnterDistance) grinExitDistance = grinEnterDistance; // enforces proper hysteresis
    }
}
