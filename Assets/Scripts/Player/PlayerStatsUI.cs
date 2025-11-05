using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    public PlayerStats stats;
    [Header("Perfil/Avatar")]
    public Image playerSprite;
    [Header("Campos de texto")]
    public TextMeshProUGUI healthText; // Vida en enteros
    public Image weaponIcon;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI attackText;

    private void OnEnable()
    {
        EventBus<bool>.Subscribe(GameEvent.PlayerStatsChanged, OnPlayerStatsChanged);
    }

    private void OnDisable()
    {
        EventBus<bool>.Unsubscribe(GameEvent.PlayerStatsChanged, OnPlayerStatsChanged);
    }
    private void OnPlayerStatsChanged(bool obj)
    {
        Refresh();
    }
    public void Refresh()
    {
        if (stats == null) return;
        if (healthText) healthText.text = $"Vida: {stats.currentHealth}/{stats.maxHealth}";

        if (stats.equippedWeapon != null)
        {
            if (weaponIcon) { weaponIcon.enabled = true; weaponIcon.sprite = stats.equippedWeapon.icon; }
            if (weaponNameText) weaponNameText.text = stats.equippedWeapon.displayName;
        }
        else
        {
            if (weaponIcon) { weaponIcon.enabled = false; weaponIcon.sprite = null; }
            if (weaponNameText) weaponNameText.text = "Sin arma";
        }

        if (attackText) attackText.text = $"Ataque: {stats.GetTotalAttack()}";
    }
}
