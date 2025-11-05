using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida basada en Image.fillAmount.
/// Ahora escucha PlayerStats a través del EventBus (HealthChanged/PlayerStatsChanged) y usa maxHealth/currentHealth.
/// </summary>
[RequireComponent(typeof(Image))]
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // si no se asigna, usa el Image del mismo GO
    [SerializeField] private int maxLives = 5; // vidas máximas visuales
    [SerializeField] private int actualLives;
    [SerializeField] private TextMeshProUGUI LivesText;
    [Header("Player Source")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private bool autoFindPlayerStats = true;

    private void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
        if (!playerStats && autoFindPlayerStats)
        {
            // Intentar obtener desde el GameManager o directamente en escena
            if (GameManager.Instance && GameManager.Instance.player)
            {
                playerStats = GameManager.Instance.player.GetComponent<PlayerStats>();
            }
            if (!playerStats)
            {
                playerStats = FindObjectOfType<PlayerStats>();
            }
        }

        int initMax = playerStats ? playerStats.maxHealth : maxLives;
        int initCurrent = playerStats ? playerStats.currentHealth : maxLives;
        UpdateUI(initCurrent, initMax);
    }

    private void OnEnable()
    {
        EventBus<int>.Subscribe(GameEvent.HealthChanged, OnHealthChanged);
        EventBus<bool>.Subscribe(GameEvent.PlayerStatsChanged, OnStatsChanged);
    }

    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(GameEvent.HealthChanged, OnHealthChanged);
        EventBus<bool>.Unsubscribe(GameEvent.PlayerStatsChanged, OnStatsChanged);
    }

    private void OnHealthChanged(int current)
    {
        int max = playerStats ? playerStats.maxHealth : maxLives;
        UpdateUI(current, max);
    }

    private void OnStatsChanged(bool _)
    {
        int max = playerStats ? playerStats.maxHealth : maxLives;
        int current = playerStats ? playerStats.currentHealth : actualLives;
        UpdateUI(current, max);
    }

    private void UpdateUI(int currentLives, int max)
    {
        currentLives = Mathf.Clamp(currentLives, 0, max);
        if (fillImage)
        {
            fillImage.fillAmount = max > 0 ? (float)currentLives / max : 0f;
        }
        if (LivesText)
        {
            LivesText.text = $"{currentLives}/{max}";
        }
        // Mantener campos locales para posibles lecturas
        actualLives = currentLives;
        // No sobreescribimos maxLives serializado, pero lo usamos como fallback cuando no hay playerStats
    }
}
