using Unity.VisualScripting;
using UnityEngine;

public class Paddle : MonoBehaviour
{
    //public float maxPaddleSpeed = 1f;
    //public float paddleForce = 1f;
    void Start()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        float max = collider.bounds.max.z;
        float min = collider.bounds.min.z;
    }

    [Header("Input Axis Name")]
    public string inputAxis = "Awesome Paddle"; // or "VerticalRightPaddle"

    [Header("Movement Settings")]
    public float speed = 5f;
    public float zMin = -4f;
    public float zMax = 4f;

    void Update()
    {
        // Read the custom axis
        float movementAxis = Input.GetAxis(inputAxis);

        // Move paddle along Z-axis
        Vector3 newPosition = transform.position
                              + new Vector3(0f, 0f, movementAxis * speed * Time.deltaTime);

        // Clamp the paddle's Z position
        newPosition.z = Mathf.Clamp(newPosition.z, zMin, zMax);

        transform.position = newPosition;
    }
}
