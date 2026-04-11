using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu: MonoBehaviour
{
    public void NewGame()
    {

        PlayerPrefs.DeleteKey("SavedPlayerX");

        PlayerPrefs.DeleteKey("SavedPlayerY");

        PlayerPrefs.Save();

        SceneManager.LoadScene("Game"); 
    }

    public void ContinueGame()
    {
        
        SceneManager.LoadScene("Game");
    }

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