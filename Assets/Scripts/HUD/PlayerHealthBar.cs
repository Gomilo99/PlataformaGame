using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida basada en Image.fillAmount.
/// Se suscribe a eventos de vidas (enteras) del GameManager y actualiza el fill.
/// </summary>
[RequireComponent(typeof(Image))]
public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // si no se asigna, usa el Image del mismo GO
    [SerializeField] private int maxLives = 5; // vidas máximas visuales

    private void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
        UpdateFill(GameManager.Instance != null ? (int)GameManager.Instance.vidas : maxLives);
    }

    private void OnEnable()
    {
        EventBus<int>.Subscribe(GameEvent.VidaPerdida, OnLivesChangedAfterLoss);
        EventBus<int>.Subscribe(GameEvent.VidaGanada, OnLivesChangedAfterGain);
    }

    private void OnDisable()
    {
        EventBus<int>.Unsubscribe(GameEvent.VidaPerdida, OnLivesChangedAfterLoss);
        EventBus<int>.Unsubscribe(GameEvent.VidaGanada, OnLivesChangedAfterGain);
    }

    private void OnLivesChangedAfterLoss(int remaining)
    {
        UpdateFill(remaining);
    }

    private void OnLivesChangedAfterGain(int beforeGain)
    {
        // GameManager publica el valor antes de sumar; sumamos 1 para el nuevo total visible
        UpdateFill(beforeGain + 1);
    }

    private void UpdateFill(int currentLives)
    {
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);
        if (!fillImage) return;
        fillImage.fillAmount = maxLives > 0 ? (float)currentLives / maxLives : 0f;
    }
}
