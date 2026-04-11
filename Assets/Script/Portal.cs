using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("Scene Destination")]
    [Tooltip("Type the exact name of the scene you want to load")]
    public string sceneToLoad;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Since your player is made of multiple bone colliders, we just need one of them to touch the portal
        if (!isTransitioning && collision.CompareTag("Player"))
        {
            if (!string.IsNullOrEmpty(sceneToLoad))
            {
                isTransitioning = true;
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogWarning("Portal: No scene specified to load in the inspector!");
            }
        }
    }
}
