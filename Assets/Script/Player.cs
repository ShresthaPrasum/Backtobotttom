using UnityEngine;
using UnityEngine.InputSystem;

public class SlingshotPlayer : MonoBehaviour
{
    [Header("Slingshot Settings")]
    public float launchPower = 15f;
    public float maxDragDistance = 3f;
    public float minDragDistance = 0.5f;
    public float grabRadius = 1.5f; // How close to the center you need to click

    [Header("Ground & Cooldown")]
    public float groundCheckRadius = 0.3f; // How far from the bones to check for ground
    public float launchCooldown = 1f; // Seconds to wait before you can launch again

    [Header("Line Indicator")]
    public Color lineColor = Color.white;
    public float lineWidth = 0.1f;

    private Rigidbody2D[] boneRigidbodies;
    private Camera cam;
    private LineRenderer aimLine;

    private bool isDragging;
    private Vector2 dragStartMousePos;
    private Vector2 currentDragVector;
    private Vector2[] boneStartPositions;
    private float nextLaunchTime;

    private void Awake()
    {
        // Automatically find all 8 of your bone rigidbodies inside this object!
        boneRigidbodies = GetComponentsInChildren<Rigidbody2D>();
        boneStartPositions = new Vector2[boneRigidbodies.Length];
        
        cam = Camera.main;

        // Create the shooting line via script automatically
        aimLine = gameObject.AddComponent<LineRenderer>();
        aimLine.positionCount = 2;
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
        aimLine.material = new Material(Shader.Find("Sprites/Default")); // Gives it a solid unlit color
        aimLine.sortingOrder = 10;
        aimLine.enabled = false;
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        if (Mouse.current == null || boneRigidbodies.Length == 0) return;

        HandleInput();
    }

    private void HandleInput()
    {
        // 1. Start Drag
        if (Mouse.current.leftButton.wasPressedThisFrame && Time.time >= nextLaunchTime)
        {
            // Don't allow dragging if we are not touching the ground/colliders!
            if (!IsGrounded()) return;

            Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            
            // Check if we clicked near the center of the bones
            if (Vector2.Distance(mousePos, GetAverageCenter()) <= grabRadius)
            {
                isDragging = true;
                dragStartMousePos = mousePos;
                currentDragVector = Vector2.zero;

                aimLine.enabled = true;

                
                for (int i = 0; i < boneRigidbodies.Length; i++)
                {
                    
                    boneRigidbodies[i].constraints = RigidbodyConstraints2D.FreezeAll;
                    boneRigidbodies[i].linearVelocity = Vector2.zero;
                    boneStartPositions[i] = boneRigidbodies[i].position;
                }
            }
        }
        // 2. Dragging
        else if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 rawDrag = mousePos - dragStartMousePos;
            currentDragVector = Vector2.ClampMagnitude(rawDrag, maxDragDistance);

            
            Vector2 centerPos = GetAverageCenter();
            aimLine.SetPosition(0, centerPos);
            aimLine.SetPosition(1, centerPos - currentDragVector);
        }
        // 3. Release Drag (Launch)
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            aimLine.enabled = false;

            bool actuallyLaunched = false;

            for (int i = 0; i < boneRigidbodies.Length; i++)
            {
                // Unfreeze the bones completely so they can fly, jiggle, and rotate naturally
                boneRigidbodies[i].constraints = RigidbodyConstraints2D.None;

                // If pulled far enough, shoot every individual bone!
                if (currentDragVector.magnitude >= minDragDistance)
                {
                    boneRigidbodies[i].AddForce(-currentDragVector * launchPower, ForceMode2D.Impulse);
                    actuallyLaunched = true;
                }
            }

            if (actuallyLaunched)
            {
                nextLaunchTime = Time.time + launchCooldown; // Lock the slingshot for designated cooldown time
            }

            currentDragVector = Vector2.zero;
        }
    }

    private Vector2 GetAverageCenter()
    {
        Vector2 center = Vector2.zero;
        foreach (var rb in boneRigidbodies)
        {
            center += rb.position;
        }
        return center / boneRigidbodies.Length;
    }

    private bool IsGrounded()
    {
        // Simple logic: Is any one of our bones physically touching ANY collider that isn't the player?
        foreach (var rb in boneRigidbodies)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(rb.position, groundCheckRadius);
            foreach (Collider2D col in colliders)
            {
                // Ignore the ground check if the detected collider is one of our own bones
                if (!col.transform.IsChildOf(this.transform))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
