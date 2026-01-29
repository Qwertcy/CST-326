using UnityEngine; // unity core types

public class LookAtTarget : MonoBehaviour // simple camera aim helper
{
    [SerializeField] private Transform target; // earth transform

    void LateUpdate() // runs after all motion, so aim is stable
    {
        if (target == null) return; // safety check
        transform.LookAt(target.position); // rotate to face earth center
    }
}
