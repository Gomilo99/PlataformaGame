using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Métodos públicos para conectar a los botones de la pantalla de pausa.
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    public GameFlowManager gameFlow;
    public string mainMenuSceneName = "MainMenu";

    private void Awake()
    {
        if (!gameFlow) gameFlow = FindObjectOfType<GameFlowManager>();
    }

    public void OnResumeClicked()
    {
        if (gameFlow) gameFlow.Resume();
    }

    public void OnRestartLevelClicked()
    {
        if (gameFlow)
        {
            gameFlow.ResetLevel();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void OnExitToMainMenuClicked()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
