using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPos, length;
    public GameObject cam;

    // The speed at which the background should move relative to the camera
    public float parallaxEffect;

    void Start()
    {
        // Store initial position
        startPos = transform.position.x;

        // Get width of the background sprite
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        // Calculate how far the background should move based on camera position
        // parallaxEffect:
        // 0   → moves fully with camera
        // 1   → does not move
        // 0.5 → moves at half speed
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        // Apply parallax movement
        transform.position = new Vector3(
            startPos + distance,
            transform.position.y,
            transform.position.z
        );

        // Infinite scrolling logic
        // If background goes out of view, reposition it
        if (movement > startPos + length)
        {
            startPos += length;
        }
        else if (movement < startPos - length)
        {
            startPos -= length;
        }
    }
}