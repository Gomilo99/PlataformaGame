using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }
    [Header("Textos HUD")]
    [Tooltip("Controlador de victoria por monedas 'x/y'.")]
    public TextMeshProUGUI monedasProgress;
    [Tooltip("Controlador de victoria por enemigos 'x/y'.")]
    public TextMeshProUGUI enemigosProgress;
    [Header("Contador de dinero")]
    public TextMeshProUGUI moneyCountText;
    [Tooltip("Objeto Vacio que contiene todos los elementos de UI en juego (monedas, enemigos, etc).")]
    public GameObject uiElementsContainer;

    [Header("Contadores internos")]
    public int moneyCounter = 0;
    public int enemigosEliminados = 0;

    [Header("Paneles UI (Canvas)")]
    public GameObject pausePanel;
    public GameObject winPanel;
    public GameObject deathPanel;

    // Ya no guardamos referencia al GameManager; usaremos GameManager.Instance directamente.

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Debug.LogWarning("Más de un HUD en escena; manteniendo el primero."); return; }
        if (!uiElementsContainer)
        {
            Debug.LogWarning("HUD: No se ha asignado uiElementsContainer en el inspector.");
        }
        if (!pausePanel)
        {
            Debug.LogWarning("HUD: No se ha asignado 'pausePanel' en el inspector.");
        }
        if (!winPanel)
        {
            Debug.LogWarning("HUD: No se ha asignado 'winPanel' en el inspector.");
        }
        if (!deathPanel)
        {
            Debug.LogWarning("HUD: No se ha asignado 'deathPanel' en el inspector.");
        }
    }

    private void OnEnable()
    {
        // Suscripciones a eventos
        EventBus<int>.Subscribe(GameEvent.CoinCollected, HandleOnCoinCollected);
        EventBus<int>.Subscribe(GameEvent.EnemyKilled, HandleOnEnemyKilled);
        EventBus<bool>.Subscribe(GameEvent.GamePaused, HandleOnGamePaused);
        EventBus<bool>.Subscribe(GameEvent.GameResumed, HandleOnGameResumed);
        EventBus<bool>.Subscribe(GameEvent.LevelReset, HandleOnLevelReset);
        EventBus<bool>.Subscribe(GameEvent.WinConditionMet, HandleOnWin);
        EventBus<bool>.Subscribe(GameEvent.PlayerDied, HandleOnPlayerDied);
        // Progreso vía eventos de texto (x/y)
        EventBus<string>.Subscribe(GameEvent.CoinsProgressUpdated, HandleOnCoinsProgressUpdated);
        EventBus<string>.Subscribe(GameEvent.EnemiesProgressUpdated, HandleOnEnemiesProgressUpdated);

        // Inicializar textos con el estado actual si ya existe GameManager
        var gm = GameManager.Instance;
        RefreshTexts(); // contador de dinero
        if (gm)
        {
            if (monedasProgress) monedasProgress.text = gm.coinsProgressText;
            if (enemigosProgress) enemigosProgress.text = gm.enemiesProgressText;
        }
    }

    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(GameEvent.CoinCollected, HandleOnCoinCollected);
        EventBus<int>.Unsubscribe(GameEvent.EnemyKilled, HandleOnEnemyKilled);
        EventBus<bool>.Unsubscribe(GameEvent.GamePaused, HandleOnGamePaused);
        EventBus<bool>.Unsubscribe(GameEvent.GameResumed, HandleOnGameResumed);
        EventBus<bool>.Unsubscribe(GameEvent.LevelReset, HandleOnLevelReset);
        EventBus<bool>.Unsubscribe(GameEvent.WinConditionMet, HandleOnWin);
        EventBus<bool>.Unsubscribe(GameEvent.PlayerDied, HandleOnPlayerDied);
        EventBus<string>.Unsubscribe(GameEvent.CoinsProgressUpdated, HandleOnCoinsProgressUpdated);
        EventBus<string>.Unsubscribe(GameEvent.EnemiesProgressUpdated, HandleOnEnemiesProgressUpdated);
    }

    private void HandleOnCoinCollected(int coinValue)
    {
        moneyCounter += coinValue;
        RefreshTexts();
    }

    private void HandleOnEnemyKilled(int count)
    {
        enemigosEliminados += count;
        RefreshTexts();
    }

    private void HandleOnGamePaused(bool _)
    {
        pausePanel.SetActive(true);
        uiElementsContainer.SetActive(false);
    }

    private void HandleOnGameResumed(bool _)
    {
        pausePanel.SetActive(false);
        uiElementsContainer.SetActive(true);
    }

    private void HandleOnLevelReset(bool _)
    {
        moneyCounter = 0;
        enemigosEliminados = 0;
        RefreshTexts();
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        deathPanel.SetActive(false);
    }

    private void HandleOnWin(bool _)
    {
        winPanel.SetActive(true);
        uiElementsContainer.SetActive(false);
    }

    private void HandleOnPlayerDied(bool _)
    {
        deathPanel.SetActive(true);
        uiElementsContainer.SetActive(false);
    }

    private void HandleOnCoinsProgressUpdated(string progress)
    {
        if (monedasProgress != null)
        {
            monedasProgress.text = progress ?? string.Empty; // Espera formato "x/y" sin etiqueta
        }
    }

    private void HandleOnEnemiesProgressUpdated(string progress)
    {
        if (enemigosProgress != null)
        {
            enemigosProgress.text = progress ?? string.Empty; // Espera formato "x/y" sin etiqueta
        }
    }

    private void RefreshTexts()
    {
        // Solo actualiza el contador de dinero inmediato; el progreso (x/y) llega por eventos
        if (moneyCountText) moneyCountText.text = moneyCounter.ToString();
    }

    // Métodos para conectar a botones del panel de pausa si prefieres centralizar en HUD
    public void UI_Resume() { if (GameManager.Instance) GameManager.Instance.Resume(); }
    public void UI_RestartLevel() { if (GameManager.Instance) GameManager.Instance.ResetLevel(); }
    public void UI_ExitToMainMenu()
    {
        // Usa GameManager (fusionado) si existe; si no, deja que tu escena de menú haga el manejo
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
