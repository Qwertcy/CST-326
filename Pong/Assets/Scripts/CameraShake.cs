using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Tuning")]
    [SerializeField] private float duration = 0.18f;
    [SerializeField] private float amplitude = 0.12f;
    [SerializeField] private float frequency = 35f;

    private Vector3 originalLocalPos; // for restoration
    private Coroutine shakeRoutine; // for stopping existing shake

    private void Awake()
    {
        originalLocalPos = transform.localPosition;
    }

    public void Shake() // default shake
    {
        Shake(duration, amplitude, frequency); // parameterized overload
    }

    public void Shake(float shakeDuration, float shakeAmplitude, float shakeFrequency) // parameterized shake trigger -> in case i want to configure a different shake later
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine); // stops any previous shake so we don't stack offsets
        shakeRoutine = StartCoroutine(ShakeRoutine(shakeDuration, shakeAmplitude, shakeFrequency)); // starts new shake coroutine
    }

    private IEnumerator ShakeRoutine(float shakeDuration, float shakeAmplitude, float shakeFrequency) // coroutine that applies offsets over time
    {
        float elapsed = 0f; // timer accumulator
        originalLocalPos = transform.localPosition; // recaching in case camera moved since last awake

        while (elapsed < shakeDuration) // runs until time is up
        {
            float damper = 1f - (elapsed / shakeDuration); // linearly fades out intensity over time
            float x = Random.Range(-1f, 1f) * shakeAmplitude * damper; // random x offset scaled by amplitude and damper
            float y = Random.Range(-1f, 1f) * shakeAmplitude * damper; // random y offset scaled by amplitude and damper

            transform.localPosition = originalLocalPos + new Vector3(x, y, 0f); // applies offset in local space

            elapsed += Time.deltaTime; // advances timer by frame time
            float wait = 1f / Mathf.Max(1f, shakeFrequency); // computes update interval from frequency (avoids divide by zero)
            yield return new WaitForSeconds(wait); // waits a bit to control jitter speed
        }

        transform.localPosition = originalLocalPos; // restores original position at end
        shakeRoutine = null; // clears routine handle
    }
}
