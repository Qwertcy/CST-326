using UnityEngine;

public class QuestionRotator : MonoBehaviour
{
    public float framesPerSecond = 8f;

    private Material instanceMaterial;

    private const int totalFrames = 5;
    private const float stepSize = 1f / totalFrames;

    void Start()
    {
        Renderer r = GetComponent<Renderer>();
        instanceMaterial = r.material; // creates unique material instance (prevents shared offsets)
        instanceMaterial.mainTextureOffset = Vector2.zero; // ensures clean starting state
    }

    void Update() // runs every frame.
    {
        float frameIndex = Mathf.Floor(Time.time * framesPerSecond) % totalFrames; // converts time into integer frame index 0–4 (wraps around with modulo)
        float yOffset = frameIndex * stepSize; // calculates exact snapped offset (0, 0.2, 0.4, 0.6, 0.8).
        instanceMaterial.mainTextureOffset = new Vector2(0f, yOffset); // applies snapped frame offset.
    }
}