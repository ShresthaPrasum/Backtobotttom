using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu: MonoBehaviour
{
    public void Play()
    {
        SceneManager.LoadScene("Game");
    }
    public void Home()
    {
        SceneManager.LoadScene("Menu");
    }
    public void Guide()
    {
        SceneManager.LoadScene("Guide");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}