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

    [Header("Dot Indicator")]
    public GameObject dotPrefab; // Assign a simple circle sprite object here
    public int numberOfDots = 8;
    public float startDotSize = 0.3f;
    public float endDotSize = 0.05f;

    private Rigidbody2D[] boneRigidbodies;
    private Camera cam;
    private GameObject[] aimDots;

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

        // Generate the pool of indicator dots
        aimDots = new GameObject[numberOfDots];
        for (int i = 0; i < numberOfDots; i++)
        {
            if (dotPrefab != null)
            {
                aimDots[i] = Instantiate(dotPrefab, Vector3.zero, Quaternion.identity);
                aimDots[i].SetActive(false);
            }
        }
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

                if (dotPrefab != null)
                {
                    foreach (var dot in aimDots) if (dot != null) dot.SetActive(true);
                }

                
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

            // Position and scale the dots to show the trajectory
            if (dotPrefab != null && currentDragVector.magnitude > 0)
            {
                Vector2 centerPos = GetAverageCenter();
                Vector2 launchDirection = -currentDragVector;

                for (int i = 0; i < numberOfDots; i++)
                {
                    if (aimDots[i] == null) continue;

                    // Space them evenly
                    float t = i / (float)Mathf.Max(1, numberOfDots - 1);
                    aimDots[i].transform.position = centerPos + (launchDirection * t);

                    // Scale from large (start) to small (end)
                    float size = Mathf.Lerp(startDotSize, endDotSize, t);
                    aimDots[i].transform.localScale = new Vector3(size, size, 1f);
                }
            }
        }
        // 3. Release Drag (Launch)
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            if (dotPrefab != null)
            {
                foreach (var dot in aimDots) if (dot != null) dot.SetActive(false);
            }

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
