using UnityEngine; // provides core unity types like transform, sprite, and vector math

public class EnemyFaceController : MonoBehaviour // swaps enemy face sprites based on player distance with hysteresis
{
    [Header("references")]
    [SerializeField] private Transform playerTransform; // reference to the player (ball) transform for distance checks
    [SerializeField] private SpriteRenderer faceRenderer; // renderer on the Visual child that displays the face sprite

    [Header("faces")]
    [SerializeField] private Sprite angrySprite; // sprite used when the enemy is "losing" (far away)
    [SerializeField] private Sprite grinSprite; // sprite used when the enemy is "about to win" (close)

    [Header("distance thresholds (hysteresis)")]
    [SerializeField] private float grinEnterDistance = 4f; // distance at/below which we enter grinning state
    [SerializeField] private float grinExitDistance = 5f; // distance at/above which we exit grinning state

    private bool isGrinning; // cached state so we don't flip sprites every frame near the boundary

    private void Awake() // runs once when the object is created, before Update
    {
        if (grinExitDistance < grinEnterDistance) // ensures hysteresis is valid (exit must be farther than enter)
        {
            grinExitDistance = grinEnterDistance; // clamps to avoid impossible state behavior if misconfigured in inspector
        }
    }

    private void Start() // runs once on the first frame; good place to initialize the correct visual state
    {
        UpdateState(forceApply: true); // initializes sprite immediately so it matches the starting distance
    }

    private void Update() // runs every frame to track player distance and update face state when needed
    {
        UpdateState(forceApply: false); // evaluates state transitions and applies sprite only on change
    }

    private void UpdateState(bool forceApply) // centralizes state logic so Start/Update share the same behavior
    {
        if (playerTransform == null || faceRenderer == null) return; // prevents null reference errors if not wired

        float distance = Vector3.Distance(transform.position, playerTransform.position); // computes world-space distance

        bool nextIsGrinning = isGrinning; // default to current state; we only change it when a threshold is crossed

        if (!isGrinning && distance <= grinEnterDistance) // if currently angry and we got close enough, enter grin
        {
            nextIsGrinning = true; // toggles state to grinning
        }
        else if (isGrinning && distance >= grinExitDistance) // if currently grinning and we moved far enough away, exit
        {
            nextIsGrinning = false; // toggles state back to angry
        }

        if (forceApply || nextIsGrinning != isGrinning) // only update visuals when state changes or on forced init
        {
            isGrinning = nextIsGrinning; // commits the new state so it persists across frames
            faceRenderer.sprite = isGrinning ? grinSprite : angrySprite; // sets sprite corresponding to the committed state
        }
    }

    private void OnValidate() // runs in editor when values change; helps catch bad thresholds early
    {
        if (grinEnterDistance < 0f) grinEnterDistance = 0f; // prevents negative distances which don't make sense
        if (grinExitDistance < grinEnterDistance) grinExitDistance = grinEnterDistance; // enforces proper hysteresis
    }
}
