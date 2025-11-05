using UnityEngine;

/// <summary>
/// Datos de ítem como ScriptableObject (crea assets desde el editor).
/// Define categoría, descripción y datos para aplicar efectos/equipar.
/// </summary>
[CreateAssetMenu(menuName = "Inventory/Item", fileName = "Item_XXX")]
public class ItemData : ScriptableObject
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

    [Header("UI")]
    [Tooltip("Prefab opcional de vista de slot para este ítem. Si se asigna, reemplaza el slotPrefab por defecto.")]
    public GameObject slotViewPrefab;
}

public enum ItemCategory { Consumable, Weapon, Key }
public enum ConsumableKind { HealthDelta, MaxHealthDelta, AttackDelta }
