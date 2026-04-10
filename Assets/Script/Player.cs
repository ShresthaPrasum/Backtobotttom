using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour
{
    [Header("Slingshot Setup")]
    [SerializeField] private float launchPower = 12f;
    [SerializeField] private float maxDrag = 3f;
    [SerializeField] private float minDrag = 0.15f;

    [Header("Soft Body Tuning")]
    [SerializeField] private float normalStiffness = 8f;
    [SerializeField] private float dragStiffness = 2f;
    [SerializeField] private float springDamping = 1f;

    [Header("Visual Squash")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float maxSquashX = 1.3f;
    [SerializeField] private float minSquashY = 0.6f;
    [SerializeField] private float squashSpeed = 15f;

    private Rigidbody2D mainRb;
    private Collider2D mainCol;
    private SpringJoint2D[] nodeSprings;
    private Camera cam;
    
    private Vector3 baseScale;
    private bool isDragging;
    private Vector2 dragStartMouseWorld; 
    private Vector2 playerStartWorld; 
    private Vector2 currentPull;

    private void Awake()
    {
        mainRb = GetComponent<Rigidbody2D>();
        mainCol = GetComponent<Collider2D>();
        cam = Camera.main;

        
        nodeSprings = GetComponentsInChildren<SpringJoint2D>(true);
        Collider2D[] allCols = GetComponentsInChildren<Collider2D>(true);

        
        for (int i = 0; i < allCols.Length; i++)
        {
            for (int j = i + 1; j < allCols.Length; j++)
            {
                Physics2D.IgnoreCollision(allCols[i], allCols[j]);
            }
        }

        
        foreach (var spring in nodeSprings)
        {
            if (spring.connectedBody != null)
            {
                spring.autoConfigureDistance = false;
                spring.distance = Vector2.Distance(spring.transform.position, spring.connectedBody.transform.position);
                spring.dampingRatio = springDamping;
                spring.frequency = normalStiffness;
            }
        }

        
        if (visualRoot != null)
        {
            baseScale = visualRoot.localScale;
        }
    }

    private void Update()
    {
        if (cam == null) cam = Camera.main;
        if (Mouse.current == null) return;

        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            
            if (mainCol.OverlapPoint(mousePos))
            {
                StartDrag();
            }
        }
        
        else if (Mouse.current.leftButton.isPressed && isDragging)
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            UpdateDrag(mousePos);
        }
        
        else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            ReleaseDrag();
        }

        
        if (visualRoot != null)
        {
            UpdateVisualSquash();
        }
    }

    private void FixedUpdate()
    {
        if (isDragging)
        {
            
            mainRb.MovePosition(playerStartWorld + currentPull);
        }
    }

    private void StartDrag()
    {
        isDragging = true;
        dragStartMouseWorld = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        playerStartWorld = mainRb.position;
        currentPull = Vector2.zero;

        
        mainRb.bodyType = RigidbodyType2D.Kinematic;
        mainRb.linearVelocity = Vector2.zero;
        mainRb.angularVelocity = 0f;

        
        SetSpringStiffness(dragStiffness);
    }

    private void UpdateDrag(Vector2 mousePos)
    {
        
        Vector2 rawPull = mousePos - dragStartMouseWorld;
        
        
        currentPull = Vector2.ClampMagnitude(rawPull, maxDrag);
    }

    private void ReleaseDrag()
    {
        isDragging = false;
        
        
        mainRb.bodyType = RigidbodyType2D.Dynamic;
        
        
        SetSpringStiffness(normalStiffness);

        
        if (currentPull.magnitude >= minDrag)
        {
            mainRb.AddForce(-currentPull * launchPower, ForceMode2D.Impulse);
        }
        else
        {
            
            mainRb.position = playerStartWorld;
        }

        currentPull = Vector2.zero;
    }

    private void SetSpringStiffness(float stiffness)
    {
        for (int i = 0; i < nodeSprings.Length; i++)
        {
            nodeSprings[i].frequency = stiffness;
        }
    }

    private void UpdateVisualSquash()
    {
        float percentPulled = currentPull.magnitude / maxDrag;

        Vector3 targetScale = baseScale;
        targetScale.x = baseScale.x * Mathf.Lerp(1f, maxSquashX, percentPulled);
        targetScale.y = baseScale.y * Mathf.Lerp(1f, minSquashY, percentPulled);

        visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, targetScale, Time.deltaTime * squashSpeed);
    }
}