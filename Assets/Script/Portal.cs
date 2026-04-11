using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string sceneToLoad;

    private bool isTransitioning = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
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
