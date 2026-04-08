using UnityEngine;

public class HorizontalParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public Transform layerTransform;
        public float parallaxFactor; // 0 = no movement, 1 = full camera movement
        [HideInInspector] public float spriteWidth;
        [HideInInspector] public Vector3 startPosition;
    }

    [SerializeField] private ParallaxLayer[] layers;
    [SerializeField] private Camera mainCamera;
    private Vector3 cameraStartPosition;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("HorizontalParallax: No camera found. Assign Main Camera in the inspector.");
            enabled = false;
            return;
        }

        cameraStartPosition = mainCamera.transform.position;

        // Cache sprite widths and starting positions
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerTransform != null)
            {
                SpriteRenderer spriteRenderer = layers[i].layerTransform.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    layers[i].spriteWidth = spriteRenderer.bounds.size.x;
                }
                layers[i].startPosition = layers[i].layerTransform.position;
            }
        }
    }

    void LateUpdate()
    {
        float cameraTravelX = mainCamera.transform.position.x - cameraStartPosition.x;

        // Apply parallax to each layer
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].layerTransform != null && layers[i].spriteWidth > 0)
            {
                float parallaxOffsetX = cameraTravelX * layers[i].parallaxFactor;
                float wrappedOffsetX = Mathf.Repeat(parallaxOffsetX, layers[i].spriteWidth);

                Vector3 newPosition = layers[i].startPosition;
                newPosition.x = layers[i].startPosition.x + wrappedOffsetX;

                layers[i].layerTransform.position = newPosition;
            }
        }
    }
}
