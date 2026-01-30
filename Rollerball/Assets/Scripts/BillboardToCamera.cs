using UnityEngine;

public class BillboardToCamera : MonoBehaviour // rotates sprite to face camera
{
    [SerializeField] private Transform cameraTransform; // camera reference assigned in inspector

    private void LateUpdate() // runs after movement for stable visuals
    {
        if (cameraTransform == null) return; // prevents null reference crashes

        Vector3 direction = transform.position - cameraTransform.position; // direction away from camera
        direction.y = 0f; // locks rotation to y-axis so face stays upright

        if (direction.sqrMagnitude < 0.0001f) return; // avoids invalid rotations

        transform.rotation = Quaternion.LookRotation(direction); // face the camera
    }
}
