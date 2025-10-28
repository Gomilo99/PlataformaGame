using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats Base")]
    public int maxHealth = 10;
    public int currentHealth = 10;
    public int baseAttack = 1;

    [Header("Equipo")]
    public ItemData equippedWeapon;

    public System.Action OnStatsChanged;

    public int GetTotalAttack()
    {
        int weaponAtk = (equippedWeapon != null) ? equippedWeapon.weaponAttack : 0;
        return baseAttack + weaponAtk;
    }

    public void ApplyConsumable(ItemData item)
    {
        if (item == null || item.category != ItemCategory.Consumable) return;
        switch (item.consumableKind)
        {
            case ConsumableKind.HealthDelta:
                currentHealth = Mathf.Clamp(currentHealth + item.amount, 0, maxHealth);
                break;
            case ConsumableKind.MaxHealthDelta:
                maxHealth = Mathf.Max(1, maxHealth + item.amount);
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                break;
            case ConsumableKind.AttackDelta:
                baseAttack = Mathf.Max(0, baseAttack + item.amount);
                break;
        }
        OnStatsChanged?.Invoke();
    }

    public void EquipWeapon(ItemData weapon)
    {
        if (weapon == null || weapon.category != ItemCategory.Weapon) return;
        equippedWeapon = weapon;
        OnStatsChanged?.Invoke();
    }
}
