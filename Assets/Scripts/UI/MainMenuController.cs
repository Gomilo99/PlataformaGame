using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de menú principal: jugar, seleccionar nivel (simple) y salir.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Tooltip("Nombre de la escena del primer nivel.")]
    public string firstLevelSceneName = "Level_01";

    public void Play()
    {
        if (!string.IsNullOrEmpty(firstLevelSceneName))
        {
            SceneManager.LoadScene(firstLevelSceneName);
        }
    }

    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
