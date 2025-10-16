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

    public enum CoinsGoalMode { Manual, ByValue, ByCount }
    [Header("Progreso y metas")]
    [Tooltip("Total de monedas requeridas para ganar (0 = ignorar). Si autoComputeCoins está activo, este valor será reemplazado al inicio.")]
    public int targetCoins = 0;
    [Tooltip("Total de enemigos a eliminar para ganar (0 = ignorar). Si autoComputeEnemies está activo, este valor será reemplazado al inicio.")]
    public int targetEnemies = 0;
    private int coinsCollected = 0;
    private int enemiesKilled = 0;

    // Paneles UI se controlan desde HUD; GameManager no activa/desactiva UI

    private bool paused = false;
    private bool won = false;
    private bool dead = false;

    [Header("Cálculo automático de metas")]
    [Tooltip("Si está activo, al iniciar se calculará automáticamente el objetivo de monedas.")]
    public bool autoComputeCoins = true;
    [Tooltip("Modo de cálculo de objetivo de monedas: por valor total de las monedas del nivel, por cantidad de monedas colocadas, o manual.")]
    public CoinsGoalMode coinsGoalMode = CoinsGoalMode.ByValue;
    [Tooltip("Si está activo, al iniciar se calculará automáticamente el objetivo de enemigos por cantidad de enemigos en la escena.")]
    public bool autoComputeEnemies = true;
    [Tooltip("Incluir objetos desactivados al calcular (útil si activas enemigos/monedas más tarde).")]
    public bool includeInactiveInCounts = true;

    [Header("Depuración")]
    public bool logComputedGoals = true;

    [Header("Contenedores de nivel")]
    [Tooltip("Transform raíz que contiene TODAS las monedas del nivel. Si se asigna, el cálculo solo buscará aquí.")]
    public Transform coinsRoot;
    [Tooltip("Transform raíz que contiene TODOS los enemigos del nivel. Si se asigna, el cálculo solo buscará aquí.")]
    public Transform enemiesRoot;

    [Header("Escenas")]
    [Tooltip("Nombre de la escena del Menú Principal para volver desde el juego.")]
    public string mainMenuSceneName = "MainMenu";
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
        // Calcula los objetivos al cargar la escena (antes de que la UI se muestre)
        ComputeLevelGoals();
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
        // Input centralizado en CharacterController.
        // Mantener vacío para lógica de frame del GameManager.
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
        EventBus<bool>.Publish(GameEvent.GamePaused, true);
    }

    public void Resume()
    {
        if (!paused) return;
        paused = false;
        Time.timeScale = 1f;
        EventBus<bool>.Publish(GameEvent.GameResumed, true);
    }

    public void ResetLevel()
    {
        Time.timeScale = 1f;
        EventBus<bool>.Publish(GameEvent.LevelReset, true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        // Reset contadores locales
        coinsCollected = 0; enemiesKilled = 0; won = false; paused = false; dead = false;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /*
    ==========================================
    Confirmación de salida (WIP, comentarizado)
    ------------------------------------------
    Descomenta esta sección si quieres mostrar un panel
    de confirmación "¿Seguro que deseas salir?" antes de
    cerrar la aplicación o volver al menú.

    [Header("Confirmación de salida (WIP)")]
    public GameObject confirmQuitPanel;

    public void ShowConfirmQuit()
    {
        Time.timeScale = 0f;
        if (confirmQuitPanel) confirmQuitPanel.SetActive(true);
    }
    public void OnConfirmQuitYes()
    {
        if (confirmQuitPanel) confirmQuitPanel.SetActive(false);
        QuitGame();
    }
    public void OnConfirmQuitNo()
    {
        if (confirmQuitPanel) confirmQuitPanel.SetActive(false);
        if (!paused) Time.timeScale = 1f;
    }
    ==========================================
    */

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
            EventBus<bool>.Publish(GameEvent.WinConditionMet, true);
        }
    }

    // --- Pantalla de muerte ---
    public void TriggerPlayerDeath()
    {
        if (dead || won) return;
        dead = true;
        Time.timeScale = 0f;
        EventBus<bool>.Publish(GameEvent.PlayerDied, true);
        Debug.Log("[GameManager] Player Died -> mostrando Death Panel y publicando evento.");
    }

    private void ComputeLevelGoals()
    {
    var scene = SceneManager.GetActiveScene();
    var roots = scene.GetRootGameObjects();

        if (autoComputeCoins)
        {
            int totalValue = 0;
            int totalCount = 0;
            // Si hay contenedor específico, solo buscamos debajo de él para optimizar
            if (coinsRoot)
            {
                var coins = coinsRoot.GetComponentsInChildren<Coin>(includeInactiveInCounts);
                foreach (var c in coins)
                {
                    totalCount++;
                    totalValue += Mathf.Max(0, c.valor);
                }
            }
            else
            {
                Debug.Log("No hay contenedor de monedas asignado.");
            }
            switch (coinsGoalMode)
            {
                case CoinsGoalMode.ByValue:
                    targetCoins = totalValue;
                    break;
                case CoinsGoalMode.ByCount:
                    targetCoins = totalCount;
                    break;
                case CoinsGoalMode.Manual:
                default:
                    break;
            }
        }

        if (autoComputeEnemies)
        {
            int totalEnemies = 0;
            if (enemiesRoot)
            {
                var enemies = enemiesRoot.GetComponentsInChildren<Enemigo>(includeInactiveInCounts);
                totalEnemies = enemies != null ? enemies.Length : 0;
            }
            else
            {
                Debug.Log("No hay contenedor de enemigos asignado.");
            }
            targetEnemies = totalEnemies;
        }

        if (logComputedGoals)
        {
            Debug.Log($"[GameManager] Goals computed -> Coins target: {targetCoins} (mode {coinsGoalMode}), Enemies target: {targetEnemies}");
        }
    }
}