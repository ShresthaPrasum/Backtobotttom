using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Player : MonoBehaviour
{
	[Header("Launch")]
	[SerializeField] private float maxDragDistance = 3f;
	[SerializeField] private float launchPower = 12f;
	[SerializeField] private float minimumLaunchDistance = 0.15f;

	[Header("Soft Body")]
	[SerializeField] private Transform visualRoot;
	[SerializeField] private float chargeSpringFrequency = 2.5f;
	[SerializeField] private float chargeSpringDamping = 0.6f;
	[SerializeField] private float normalSpringFrequency = 7f;
	[SerializeField] private float normalSpringDamping = 1f;
	[SerializeField] private float maxSquashX = 1.25f;
	[SerializeField] private float minSquashY = 0.65f;
	[SerializeField] private float squashLerpSpeed = 14f;
	[SerializeField] private float wobbleStrength = 0.08f;
	[SerializeField] private float wobbleSpring = 18f;
	[SerializeField] private float wobbleDamping = 8f;
	[SerializeField] private float dragVisualOffset = 0.18f;

	private Rigidbody2D playerBody;
	private Collider2D playerCollider;
	private Camera mainCamera;
	private Rigidbody2D[] softBodyRigidbodies;
	private SpringJoint2D[] softBodySprings;

	private Vector3 baseScale;
	private Vector3 baseVisualLocalPosition;
	private Vector2 visualOffsetVelocity;
	private float visualSquashVelocity;
	private Vector2 dragOrigin;
	private Vector2 dragStartWorld;
	private Vector2 currentDragVector;
	private bool isDragging;

	private void Awake()
	{
		playerBody = GetComponent<Rigidbody2D>();
		playerCollider = GetComponent<Collider2D>();
		mainCamera = Camera.main;
		softBodyRigidbodies = GetComponentsInChildren<Rigidbody2D>(true);
		softBodySprings = GetComponentsInChildren<SpringJoint2D>(true);
		
		// Prevent self-colliding explosions between the root and the soft body nodes
		Collider2D[] allColliders = GetComponentsInChildren<Collider2D>(true);
		for (int i = 0; i < allColliders.Length; i++)
		{
			for (int j = i + 1; j < allColliders.Length; j++)
			{
				Physics2D.IgnoreCollision(allColliders[i], allColliders[j]);
			}
		}

		baseScale = transform.localScale;
		if (visualRoot == null)
		{
			visualRoot = transform;
			Debug.LogWarning("Player: visualRoot is not assigned! Wobble effects disabled to prevent physics glitches. Assign a child Sprite object to visualRoot.");
		}
		baseVisualLocalPosition = visualRoot.localPosition;

		playerBody.freezeRotation = true;
	}

	private void Update()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
			if (mainCamera == null)
			{
				return;
			}
		}

		if (!isDragging)
		{
			if (PointerPressedThisFrame())
			{
				TryStartDrag();
			}
		}
		else
		{
			if (PointerReleasedThisFrame())
			{
				ReleaseDrag();
			}
			else
			{
				UpdateDrag();
			}
		}

		UpdateSquashVisuals();
	}

	private void FixedUpdate()
	{
		if (!isDragging)
		{
			ApplyWobbleFromMotion();
			return;
		}

		playerBody.MovePosition(dragOrigin + currentDragVector);
	}

	private void TryStartDrag()
	{
		Vector2 pointerWorld = GetPointerWorldPosition();

		if (!playerCollider.OverlapPoint(pointerWorld))
		{
			return;
		}

		isDragging = true;
		dragOrigin = playerBody.position;
		dragStartWorld = pointerWorld;
		currentDragVector = Vector2.zero;

		SetSpringRigTuning(chargeSpringFrequency, chargeSpringDamping);
		playerBody.linearVelocity = Vector2.zero;
		playerBody.angularVelocity = 0f;
		playerBody.bodyType = RigidbodyType2D.Kinematic;
	}

	private void UpdateDrag()
	{
		Vector2 pointerWorld = GetPointerWorldPosition();
		Vector2 rawDragVector = dragStartWorld - pointerWorld;
		currentDragVector = Vector2.ClampMagnitude(rawDragVector, maxDragDistance);
	}

	private void ReleaseDrag()
	{
		isDragging = false;
		playerBody.bodyType = RigidbodyType2D.Dynamic;
		SetSpringRigTuning(normalSpringFrequency, normalSpringDamping);

		if (currentDragVector.magnitude < minimumLaunchDistance)
		{
			playerBody.position = dragOrigin;
			currentDragVector = Vector2.zero;
			return;
		}

		playerBody.linearVelocity = Vector2.zero;
		playerBody.angularVelocity = 0f;
		playerBody.AddForce(-currentDragVector * launchPower, ForceMode2D.Impulse);

		currentDragVector = Vector2.zero;
	}

	private void SetSpringRigTuning(float frequency, float dampingRatio)
	{
		if (softBodySprings == null || softBodySprings.Length == 0)
		{
			return;
		}

		for (int i = 0; i < softBodySprings.Length; i++)
		{
			SpringJoint2D spring = softBodySprings[i];
			if (spring == null)
			{
				continue;
			}

			spring.frequency = frequency;
			spring.dampingRatio = dampingRatio;
		}
	}

	private void UpdateSquashVisuals()
	{
		float dragPercent = Mathf.Clamp01(currentDragVector.magnitude / maxDragDistance);
		Vector2 dragDirection = currentDragVector.sqrMagnitude > 0.0001f ? currentDragVector.normalized : Vector2.zero;

		Vector3 targetScale = baseScale;
		targetScale.x = baseScale.x * Mathf.Lerp(1f, maxSquashX, dragPercent);
		targetScale.y = baseScale.y * Mathf.Lerp(1f, minSquashY, dragPercent);

		// only apply squash visually on child root, not on physics root
		if (visualRoot != transform)
		{
			visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, targetScale, Time.deltaTime * squashLerpSpeed);

			Vector3 targetVisualPosition = baseVisualLocalPosition + (Vector3)(-dragDirection * dragPercent * dragVisualOffset);
			visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, targetVisualPosition, Time.deltaTime * squashLerpSpeed);
		}
	}

	private void ApplyWobbleFromMotion()
	{
		if (visualRoot == transform) return; // Prevent breaking physics parent

		Vector2 bodyVelocity = playerBody.linearVelocity;
		float speed = bodyVelocity.magnitude;
		if (speed < 0.01f)
		{
			visualOffsetVelocity = Vector2.Lerp(visualOffsetVelocity, Vector2.zero, Time.fixedDeltaTime * wobbleDamping);
			visualSquashVelocity = Mathf.Lerp(visualSquashVelocity, 0f, Time.fixedDeltaTime * wobbleDamping);
			visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, baseVisualLocalPosition, Time.fixedDeltaTime * wobbleSpring);
			visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, baseScale, Time.fixedDeltaTime * wobbleSpring);
			return;
		}

		Vector2 motionDirection = bodyVelocity.normalized;
		Vector2 targetOffset = -motionDirection * Mathf.Min(speed * 0.015f, wobbleStrength);
		visualOffsetVelocity = Vector2.Lerp(visualOffsetVelocity, targetOffset, Time.fixedDeltaTime * wobbleSpring);
		visualSquashVelocity = Mathf.Lerp(visualSquashVelocity, Mathf.Clamp(speed * 0.01f, 0f, wobbleStrength), Time.fixedDeltaTime * wobbleSpring);

		Vector3 positionTarget = baseVisualLocalPosition + (Vector3)visualOffsetVelocity;
		Vector3 scaleTarget = baseScale + new Vector3(visualSquashVelocity, -visualSquashVelocity * 0.75f, 0f);

		visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, positionTarget, Time.fixedDeltaTime * wobbleSpring);
		visualRoot.localScale = Vector3.Lerp(visualRoot.localScale, scaleTarget, Time.fixedDeltaTime * wobbleSpring);
	}

	private Vector2 GetPointerWorldPosition()
	{
		Vector3 screenPosition = GetPointerScreenPosition();
		screenPosition.z = -mainCamera.transform.position.z;
		return mainCamera.ScreenToWorldPoint(screenPosition);
	}

	private static Vector2 GetPointerScreenPosition()
	{
#if ENABLE_INPUT_SYSTEM
		if (Mouse.current != null)
		{
			return Mouse.current.position.ReadValue();
		}

		if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
		{
			return Touchscreen.current.primaryTouch.position.ReadValue();
		}

		return Vector2.zero;
#else
		return Input.mousePosition;
#endif
	}

	private static bool PointerPressedThisFrame()
	{
#if ENABLE_INPUT_SYSTEM
		return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
		return Input.GetMouseButtonDown(0);
#endif
	}

	private static bool PointerReleasedThisFrame()
	{
#if ENABLE_INPUT_SYSTEM
		return Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame;
#else
		return Input.GetMouseButtonUp(0);
#endif
	}
}
