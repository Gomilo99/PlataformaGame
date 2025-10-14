using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controla pausa, reanudar, reinicio de nivel y condición de victoria basada en colecciones.
/// Expone métodos públicos fáciles de conectar desde botones UI.
/// </summary>
public class GameFlowManager : MonoBehaviour
{
    [Header("Conteo objetivo")]
    [Tooltip("Total de monedas requeridas para ganar")]
    public int targetCoins = 0;
    [Tooltip("Total de enemigos que deben ser eliminados para ganar")]
    public int targetEnemies = 0;

    [Header("Paneles UI (opcionales)")]
    public GameObject pausePanel;
    public GameObject winPanel;

    private int coinsCollected = 0;
    private int enemiesKilled = 0;
    private bool paused = false;
    private bool won = false;

    void OnEnable()
    {
        EventBus<int>.Subscribe(GameEvent.CoinCollected, OnCoinCollected);
        EventBus<int>.Subscribe(GameEvent.EnemyKilled, OnEnemyKilled);
    }

    void OnDisable()
    {
        EventBus<int>.Unsubscribe(GameEvent.CoinCollected, OnCoinCollected);
        EventBus<int>.Unsubscribe(GameEvent.EnemyKilled, OnEnemyKilled);
    }

    // UI hooks
    public void TogglePause()
    {
        if (won) return;
        if (paused) Resume(); else Pause();
    }

    public void Pause()
    {
        if (paused) return;
        paused = true;
        Time.timeScale = 0f;
        if (pausePanel) pausePanel.SetActive(true);
        EventBus<bool>.Publish(GameEvent.GamePaused, true);
    }

    public void Resume()
    {
        if (!paused) return;
        paused = false;
        Time.timeScale = 1f;
        if (pausePanel) pausePanel.SetActive(false);
        EventBus<bool>.Publish(GameEvent.GameResumed, true);
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        EventBus<bool>.Publish(GameEvent.LevelReset, true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnCoinCollected(int value)
    {
        coinsCollected += value;
        CheckWin();
    }

    private void OnEnemyKilled(int count)
    {
        enemiesKilled += count;
        CheckWin();
    }

    private void CheckWin()
    {
        if (won) return;
        bool coinsOk = targetCoins <= 0 || coinsCollected >= targetCoins;
        bool enemiesOk = targetEnemies <= 0 || enemiesKilled >= targetEnemies;
        if (coinsOk && enemiesOk)
        {
            won = true;
            Time.timeScale = 0f;
            if (winPanel) winPanel.SetActive(true);
            EventBus<bool>.Publish(GameEvent.WinConditionMet, true);
        }
    }
}
