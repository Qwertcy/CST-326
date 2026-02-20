using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // player
    public Vector3 offset = new Vector3(0f, 2f, -10f);

    private float currentMaxX;

    private void Start()
    {
        // camera starting position
        currentMaxX = transform.position.x;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            // player position + offset
            float desiredX = target.position.x + offset.x;
            float desiredY = target.position.y + offset.y;
            float desiredZ = offset.z;

            // only updates on moving right
            if (desiredX > currentMaxX)
            {
                currentMaxX = desiredX;
            }

            // farthest right went
            transform.position = new Vector3(currentMaxX, desiredY, desiredZ);
        }
    }
}
