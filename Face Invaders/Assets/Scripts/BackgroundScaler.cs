using UnityEngine;

public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        float worldHeight = Camera.main.orthographicSize * 2f; // calculates the total height of the camera view in world units (orthographicSize is half the height)

        float worldWidth = worldHeight * Camera.main.aspect; // calculates the total width of the camera view using the screen aspect ratio

        Vector2 spriteSize = sr.sprite.bounds.size; // gets the original size of the sprite in world units

        transform.localScale = new Vector3( // scales the background so it fills the entire camera area
            worldWidth / spriteSize.x, // scale factor needed to stretch sprite to camera width
            worldHeight / spriteSize.y, // scale factor needed to stretch sprite to camera height
            1 // z scale stays 1 because this is a 2d sprite
        );
    }
}