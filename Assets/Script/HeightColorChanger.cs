using UnityEngine;
using UnityEngine.UI; // Required for Image

public class HeightColorChanger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player to track")]
    public Transform player;
    
    [Tooltip("Assign this if you are changing a Canvas UI Image")]
    public Image targetImage;
    
    [Tooltip("Assign this if you are changing a 2D Sprite in the scene")]
    public SpriteRenderer targetSprite;

    [Header("Color Settings")]
    [Tooltip("How much Red, Green, Blue, and Alpha to add for every 1 unit the player moves UP. (Note: Unity colors go from 0.0 to 1.0, not 0 to 255. So 0.1 is a 10% change)")]
    public Color colorAddedPerUnit = new Color(-0.02f, -0.02f, -0.02f, 0f);

    private float startY;
    private Color startColor;

    private void Start()
    {
        if (player != null)
        {
            // Record where the player started so the color change is relative to the start
            startY = player.position.y;
        }

        // Automatically grab the starting color from the assigned image/sprite
        if (targetImage != null) startColor = targetImage.color;
        else if (targetSprite != null) startColor = targetSprite.color;
        else startColor = Color.white;
    }

    private void Update()
    {
        if (player == null) return;

        // Calculate how far the player has moved up from their starting position
        float heightGained = player.position.y - startY;

        // Calculate new color
        Color newColor = startColor + (colorAddedPerUnit * heightGained);

        // Unity colors must be clamped between 0 and 1 so they don't break
        newColor.r = Mathf.Clamp01(newColor.r);
        newColor.g = Mathf.Clamp01(newColor.g);
        newColor.b = Mathf.Clamp01(newColor.b);
        newColor.a = Mathf.Clamp01(newColor.a);

        // Apply it
        ApplyColor(newColor);
    }

    private void ApplyColor(Color colorToApply)
    {
        if (targetImage != null)
        {
            targetImage.color = colorToApply;
        }
        
        if (targetSprite != null)
        {
            targetSprite.color = colorToApply;
        }
    }
}
