using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class PlayerBoundsClamp : MonoBehaviour
{
    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        if (!cam.orthographic) return;

        
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // The camera’s center in X is at cam.transform.position.x
        float leftBound = cam.transform.position.x - halfWidth;
        float rightBound = cam.transform.position.x + halfWidth;

        // clamp y
        // float bottomBound = cam.transform.position.y - halfHeight;
        // float topBound = cam.transform.position.y + halfHeight;

        // player position
        Vector3 clampedPos = transform.position;

        // clamp x
        clampedPos.x = Mathf.Clamp(clampedPos.x, leftBound, rightBound);

        // clamp y
        // clampedPos.y = Mathf.Clamp(clampedPos.y, bottomBound, topBound);

        transform.position = clampedPos;
    }
}
