using UnityEngine;
using UnityEngine.InputSystem;

public class HammerController : MonoBehaviour
{
    [Header("References")]
    public Transform playerBodyTransform;

    public Rigidbody2D hammerRigidbody;

    [Header("Visual Offset")]
    public Transform hammerVisual;

    public Vector2 hammerVisualLocalOffset = new Vector2(0f, -0.35f);

    
    [Header("Hammer Reach")]

    public float armLength = 2.5f;
    
    [Header("Movement Smoothing")]

    public float moveSpeed = 15f;
    

    private Camera mainCamera;
    

    private void Start()
    {

        mainCamera = Camera.main;

        
        
        if (playerBodyTransform == null)
            playerBodyTransform = transform.parent.Find("PlayerBody");
        if (hammerRigidbody == null)
            hammerRigidbody = GetComponent<Rigidbody2D>();

        if (hammerVisual != null)
            hammerVisual.localPosition = hammerVisualLocalOffset;
    }
    
    private void FixedUpdate()
    {
        if (Mouse.current == null)
            return;

        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f; 
        

        
        Vector3 playerPos = playerBodyTransform.position;
        Vector3 directionToMouse = (mouseWorldPos - playerPos).normalized;
        
        
        Vector3 targetHammerPos = playerPos + directionToMouse * armLength;
        
        
        float step = moveSpeed * Time.fixedDeltaTime;
        Vector2 newPos = Vector2.MoveTowards(hammerRigidbody.position, targetHammerPos, step);

        hammerRigidbody.MovePosition(newPos);
        
        
        float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

        hammerRigidbody.MoveRotation(angle);
    }
}