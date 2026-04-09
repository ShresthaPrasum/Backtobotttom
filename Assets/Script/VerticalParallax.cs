using UnityEngine;

public class VerticalParallax : MonoBehaviour
{
    private float startPos;
    private float length;

    [Header("References")]
    public GameObject cam;

    [Header("Settings")]
    [Tooltip("0 = Moves with camera (foreground). 1 = Static (far background).")]
    public float parallaxEffect;

    [Tooltip("Check this if you see gaps. It automatically creates repeating clones.")]
    public bool autoDuplicate = true;

    void Start()
    {
        if (cam == null && Camera.main != null) 
            cam = Camera.main.gameObject;

        startPos = transform.position.y;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Get the vertical height of the sprite
            length = sr.bounds.size.y;
            
            if (autoDuplicate)
            {
                // Create a clone above and below to prevent ANY gaps when the sprite loops
                CreateClone(length);
                CreateClone(-length);
            }
        }
    }

    void CreateClone(float offsetY)
    {
        // Creates a child object with the identical sprite settings
        GameObject clone = new GameObject(gameObject.name + "_Clone");
        clone.transform.SetParent(transform);
        clone.transform.position = transform.position + new Vector3(0, offsetY, 0);
        clone.transform.localScale = Vector3.one;

        SpriteRenderer mySr = GetComponent<SpriteRenderer>();
        SpriteRenderer cloneSr = clone.AddComponent<SpriteRenderer>();
        
        cloneSr.sprite = mySr.sprite;
        cloneSr.color = mySr.color;
        cloneSr.sortingLayerID = mySr.sortingLayerID;
        cloneSr.sortingOrder = mySr.sortingOrder;
        cloneSr.material = mySr.material;
    }

    void LateUpdate()
    {
        if (cam == null || length == 0) return;

        // How far the camera has moved relative to the parallax effect
        float temp = (cam.transform.position.y * (1 - parallaxEffect));
        
        // How far the background should actually move
        float distance = (cam.transform.position.y * parallaxEffect);

        // Move the background
        transform.position = new Vector3(transform.position.x, startPos + distance, transform.position.z);

        // Infinite Looping Logic:
        // If the camera moves past the sprite's height, snap the parent upward!
        if (temp > startPos + length)
        {
            startPos += length;
        }
        // If the camera falls below the sprite's height, snap the parent downward
        else if (temp < startPos - length)
        {
            startPos -= length;
        }
    }
}