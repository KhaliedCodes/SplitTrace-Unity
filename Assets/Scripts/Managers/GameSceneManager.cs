using System.Collections;
using UnityEngine;
public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void LoadLobby()
    {
        // Load the lobby scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
    
    public void LoadMainMenu()
    {
        // Load the lobby scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        Time.timeScale = 1f; // Reset time scale to normal
    }

    public void Load3rdLevel()
    {
        // Load the lobby scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(3);
        Time.timeScale = 1f; // Reset time scale to normal
    }
    public void Exit()
    {
        // Load the lobby scene
        Application.Quit();
    }

}
