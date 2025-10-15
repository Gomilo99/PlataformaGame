using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [Header("Textos HUD")]
    [Tooltip("Texto para las monedas (puede reutilizarse el existente).")]
    public TextMeshProUGUI puntos; // Monedas
    [Tooltip("Texto para enemigos eliminados (opcional).")]
    public TextMeshProUGUI enemigosText;
    [Tooltip("Texto o indicador de Pausa (opcional).")]
    public TextMeshProUGUI pausaText;

    [Header("Contadores internos")]
    public int puntosTotales = 0;
    public int enemigosEliminados = 0;

    private GameFlowManager _flow;
    private GameManager _gm;

    private void Awake()
    {
        _flow = FindObjectOfType<GameFlowManager>();
        _gm = GameManager.Instance ? GameManager.Instance : FindObjectOfType<GameManager>();
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
        if (pausaText) pausaText.gameObject.SetActive(true);
    }

    private void HandleOnGameResumed(bool _)
    {
        if (pausaText) pausaText.gameObject.SetActive(false);
    }

    private void HandleOnLevelReset(bool _)
    {
        puntosTotales = 0;
        enemigosEliminados = 0;
        RefreshTexts();
        if (pausaText) pausaText.gameObject.SetActive(false);
    }

    private void HandleOnWin(bool _)
    {
        // Si tienes un texto de win aquí, podrías mostrarlo. GameFlowManager ya maneja winPanel.
    }

    private void RefreshTexts()
    {
        if (puntos)
        {
            int target = _flow ? _flow.targetCoins : (_gm ? _gm.targetCoins : 0);
            puntos.text = target > 0 ? $"Monedas: {puntosTotales}/{target}" : $"Monedas: {puntosTotales}";
        }
        if (enemigosText)
        {
            int targetE = _flow ? _flow.targetEnemies : (_gm ? _gm.targetEnemies : 0);
            enemigosText.text = targetE > 0 ? $"Enemigos: {enemigosEliminados}/{targetE}" : $"Enemigos: {enemigosEliminados}";
        }
    }

    // Métodos para conectar a botones del panel de pausa si prefieres centralizar en HUD
    public void UI_Resume() { if (_gm) _gm.Resume(); else if (_flow) _flow.Resume(); }
    public void UI_RestartLevel() { if (_gm) _gm.ResetLevel(); else if (_flow) _flow.ResetLevel(); }
    public void UI_ExitToMainMenu()
    {
        // Usa GameManager (fusionado) si existe; si no, deja que tu escena de menú haga el manejo
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
