using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public HUD hud;
    public static GameManager Instance { get; private set; }
    public CharacterController player;
    public float vidas = 3;
    public float VidasTotales;
    int actualscene;

    [Header("Progreso y metas")]
    [Tooltip("Total de monedas requeridas para ganar (0 = ignorar)")]
    public int targetCoins = 0;
    [Tooltip("Total de enemigos a eliminar para ganar (0 = ignorar)")]
    public int targetEnemies = 0;
    private int coinsCollected = 0;
    private int enemiesKilled = 0;

    [Header("UI Panels (opcionales)")]
    public GameObject pausePanel;
    public GameObject winPanel;

    private bool paused = false;
    private bool won = false;
    void Awake()
    {
        VidasTotales = vidas;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Más de un Game Manager en escena!");
        }
        actualscene = SceneManager.GetActiveScene().buildIndex;
    }

    private void OnEnable()
    {
        EventBus<int>.Subscribe(GameEvent.CoinCollected, OnCoinCollected);
        EventBus<int>.Subscribe(GameEvent.EnemyKilled, OnEnemyKilled);
    }

    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(GameEvent.CoinCollected, OnCoinCollected);
        EventBus<int>.Unsubscribe(GameEvent.EnemyKilled, OnEnemyKilled);
    }

    private void Update()
    {
        // Captura ESC para pausar/reanudar
        if (!won && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
    public void PerderVida(float dano)
    {
        vidas -= dano;
        if (vidas == 0)
        {
            //Reiniciar Nivel
            Debug.Log("Game Over");
            SceneManager.LoadScene(actualscene);
        }
        EventBus<int>.Publish(GameEvent.VidaPerdida, (int)vidas);
    }
    public bool GanarVida()
    {
        if (vidas == VidasTotales) return false;

        EventBus<int>.Publish(GameEvent.VidaGanada, (int)vidas);
        vidas += 1;
        return true;
    }

    // --- Lógica fusionada de GameFlowManager ---
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
        // Reset contadores locales
        coinsCollected = 0; enemiesKilled = 0; won = false; paused = false;
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