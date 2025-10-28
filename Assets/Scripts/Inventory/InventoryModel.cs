using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modelo de inventario simple basado en slots con stack. Emite evento OnChanged.
/// </summary>
[CreateAssetMenu(menuName = "PlataformaGame/Inventory/InventoryModel", fileName = "InventoryModel")]
public class InventoryModel : ScriptableObject
{
    [Serializable]
    public class Slot
    {
        public ItemData item;
        public int count;
    }

    public int capacity = 12;
    public List<Slot> slots = new List<Slot>();

    public event Action OnChanged;

    private void OnEnable()
    {
        if (slots == null || slots.Count != capacity)
        {
            slots = new List<Slot>(capacity);
            for (int i = 0; i < capacity; i++) slots.Add(new Slot());
        }
    }

    public bool Add(ItemData data, int amount = 1)
    {
        if (data == null || amount <= 0) return false;
        // 1) Intentar apilar en slots existentes
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            var s = slots[i];
            if (s.item == data && s.count < data.maxStack)
            {
                int canAdd = Mathf.Min(data.maxStack - s.count, amount);
                s.count += canAdd;
                amount -= canAdd;
            }
        }
        // 2) Usar slots vacíos
        for (int i = 0; i < slots.Count && amount > 0; i++)
        {
            var s = slots[i];
            if (s.item == null)
            {
                int put = Mathf.Min(data.maxStack, amount);
                s.item = data;
                s.count = put;
                amount -= put;
            }
        }
        bool changed = amount < 1;
        if (changed) OnChanged?.Invoke();
        return changed;
    }

    public bool Remove(string itemId, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;
        int remaining = amount;
        for (int i = 0; i < slots.Count && remaining > 0; i++)
        {
            var s = slots[i];
            if (s.item != null && s.item.id == itemId)
            {
                int take = Mathf.Min(s.count, remaining);
                s.count -= take;
                remaining -= take;
                if (s.count <= 0) { s.item = null; s.count = 0; }
            }
        }
        bool changed = remaining < amount;
        if (changed) OnChanged?.Invoke();
        return changed;
    }

    public InventoryModel.Slot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count) return null;
        return slots[index];
    }

    public bool RemoveAt(int index, int amount = 1)
    {
        if (index < 0 || index >= slots.Count || amount <= 0) return false;
        var s = slots[index];
        if (s.item == null || s.count <= 0) return false;
        int take = Mathf.Min(s.count, amount);
        s.count -= take;
        if (s.count <= 0) { s.item = null; s.count = 0; }
        OnChanged?.Invoke();
        return true;
    }
}
