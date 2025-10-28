using UnityEngine;

/// <summary>
/// Datos de ítem. Define categoría, descripción y datos para aplicar efectos/equipar.
/// </summary>
[System.Serializable]
public class ItemData
{
    public string id;
    public string displayName;
    [TextArea]
    public string description;
    public Sprite icon;
    public int maxStack = 99;

    public ItemCategory category = ItemCategory.Consumable;

    // Consumible
    public ConsumableKind consumableKind = ConsumableKind.HealthDelta;
    public int amount = 0; // cantidad que suma/resta/aplica

    // Arma
    public int weaponAttack = 0;
}

public enum ItemCategory { Consumable, Weapon, Key }
public enum ConsumableKind { HealthDelta, MaxHealthDelta, AttackDelta }
