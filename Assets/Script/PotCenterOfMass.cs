using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PotCenterOfMass : MonoBehaviour
{
    [SerializeField] private Vector2 centerOfMassOffset = new Vector2(0f, -0.2f);

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.centerOfMass = centerOfMassOffset;
    }
}
