using UnityEngine;

/// <summary>
/// Escucha la tecla Escape y llama a GameFlowManager para pausar/reanudar.
/// Adjunta este script a un GameObject en la escena (p.ej. un "Systems").
/// </summary>
public class PauseInput : MonoBehaviour
{
    [Tooltip("Referencia al GameFlowManager de la escena.")]
    public GameFlowManager gameFlow;

    [Tooltip("Panel/ventana de pausa; opcional si el GameFlowManager ya la muestra.")]
    public GameObject pausePanel;

    private void Awake()
    {
        if (!gameFlow)
        {
            gameFlow = FindObjectOfType<GameFlowManager>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameFlow != null)
            {
                gameFlow.TogglePause();
            }
            if (pausePanel)
            {
                // Sincroniza visibilidad si quieres forzar el panel específico
                bool shouldShow = Time.timeScale == 0f;
                if (pausePanel.activeSelf != shouldShow) pausePanel.SetActive(shouldShow);
            }
        }
    }
}
