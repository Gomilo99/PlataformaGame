using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats Base")]
    public int maxHealth = 10;
    public int currentHealth = 10;
    public int baseAttack = 1;

    [Header("Equipo")]
    public ItemData equippedWeapon;

    public int GetTotalAttack()
    {
        int weaponAtk = (equippedWeapon != null) ? equippedWeapon.weaponAttack : 0;
        return baseAttack + weaponAtk;
    }

    public void TakeDamage(int dmg)
    {
        currentHealth = Mathf.Clamp(currentHealth - Mathf.Max(0, dmg), 0, maxHealth);
        EventBus<int>.Publish(GameEvent.HealthChanged, currentHealth);
        EventBus<bool>.Publish(GameEvent.PlayerStatsChanged, true);
    }

    public void Heal(int amt)
    {
        currentHealth = Mathf.Clamp(currentHealth + Mathf.Max(0, amt), 0, maxHealth);
        EventBus<int>.Publish(GameEvent.HealthChanged, currentHealth);
        EventBus<bool>.Publish(GameEvent.PlayerStatsChanged, true);
    }

    public void ApplyConsumable(ItemData item)
    {
        if (item == null || item.category != ItemCategory.Consumable) return;
        switch (item.consumableKind)
        {
            case ConsumableKind.HealthDelta:
                currentHealth = Mathf.Clamp(currentHealth + item.amount, 0, maxHealth);
                EventBus<int>.Publish(GameEvent.HealthChanged, currentHealth);
                break;
            case ConsumableKind.MaxHealthDelta:
                maxHealth = Mathf.Max(1, maxHealth + item.amount);
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
                EventBus<int>.Publish(GameEvent.HealthChanged, currentHealth);
                break;
            case ConsumableKind.AttackDelta:
                baseAttack = Mathf.Max(0, baseAttack + item.amount);
                break;
        }
        EventBus<bool>.Publish(GameEvent.PlayerStatsChanged, true);
    }

    public void EquipWeapon(ItemData weapon)
    {
        if (weapon == null || weapon.category != ItemCategory.Weapon) return;
        equippedWeapon = weapon;
        EventBus<ItemData>.Publish(GameEvent.WeaponEquipped, equippedWeapon);
        EventBus<bool>.Publish(GameEvent.PlayerStatsChanged, true);
    }
}
