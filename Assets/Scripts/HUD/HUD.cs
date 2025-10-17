using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }
    [Header("Textos HUD")]
    [Tooltip("Texto para las monedas (puede reutilizarse el existente).")]
    public TextMeshProUGUI puntos; // Monedas
    [Tooltip("Texto para enemigos eliminados (opcional).")]
    public TextMeshProUGUI enemigosText;
    [Tooltip("Solo números 'x/y' de monedas (sin etiqueta). Opcional.")]
    public TextMeshProUGUI monedasProgress;
    [Tooltip("Solo números 'x/y' de enemigos (sin etiqueta). Opcional.")]
    public TextMeshProUGUI enemigosProgress;
    [Tooltip("Texto combinado de condiciones (p.ej. '3/5  |  1/2'). Opcional.")]
    public TextMeshProUGUI condicionesVictoriaText;
    [Tooltip("Texto o indicador de Pausa (opcional).")]
    public TextMeshProUGUI pausaText;

    [Header("Contadores internos")]
    public int puntosTotales = 0;
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

        RefreshTexts();
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
    }

    private void HandleOnCoinCollected(int coinValue)
    {
        puntosTotales += coinValue;
        RefreshTexts();
    }

    private void HandleOnEnemyKilled(int count)
    {
        enemigosEliminados += count;
        RefreshTexts();
    }

    private void HandleOnGamePaused(bool _)
    {
        if (pausePanel) pausePanel.SetActive(true);
        if (pausaText) pausaText.gameObject.SetActive(true);
    }

    private void HandleOnGameResumed(bool _)
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (pausaText) pausaText.gameObject.SetActive(false);
    }

    private void HandleOnLevelReset(bool _)
    {
        puntosTotales = 0;
        enemigosEliminados = 0;
        RefreshTexts();
        if (pausePanel) pausePanel.SetActive(false);
        if (winPanel) winPanel.SetActive(false);
        if (deathPanel) deathPanel.SetActive(false);
        if (pausaText) pausaText.gameObject.SetActive(false);
    }

    private void HandleOnWin(bool _)
    {
        if (winPanel) winPanel.SetActive(true);
    }

    private void HandleOnPlayerDied(bool _)
    {
        if (deathPanel) deathPanel.SetActive(true);
    }

    private void RefreshTexts()
    {
        var gm = GameManager.Instance;
        if (puntos)
        {
            int target = gm ? gm.targetCoins : 0;
            puntos.text = target > 0 ? $"Monedas: {puntosTotales}/{target}" : $"Monedas: {puntosTotales}";
        }
        if (monedasProgress && gm)
        {
            monedasProgress.text = gm.coinsProgressText;
        }
        if (enemigosText)
        {
            int targetE = gm ? gm.targetEnemies : 0;
            enemigosText.text = targetE > 0 ? $"Enemigos: {enemigosEliminados}/{targetE}" : $"Enemigos: {enemigosEliminados}";
        }
        if (enemigosProgress && gm)
        {
            enemigosProgress.text = gm.enemiesProgressText;
        }
        if (condicionesVictoriaText && gm)
        {
            condicionesVictoriaText.text = gm.winConditionsText;
        }
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
