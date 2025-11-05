using UnityEngine;

public class InventoryRuntime : MonoBehaviour
{
    public static InventoryRuntime Instance { get; private set; }

    [Header("Modelo de inventario en uso")]
    public InventoryModel model;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Más de un InventoryRuntime en escena; destruyendo el nuevo.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (model == null)
        {
            Debug.LogWarning("InventoryRuntime: no hay InventoryModel asignado.");
        }
    }

    public bool AddItem(ItemData item, int amount = 1)
    {
        if (model == null) { Debug.LogWarning("InventoryRuntime.AddItem: model nulo"); return false; }
        return model.Add(item, amount);
    }

    public bool RemoveItemById(string itemId, int amount = 1)
    {
        if (model == null) { Debug.LogWarning("InventoryRuntime.RemoveItemById: model nulo"); return false; }
        return model.Remove(itemId, amount);
    }

    // Accesos de prueba desde el editor (clic derecho en el componente)
    [ContextMenu("Test/Add Selected Item (1)")]
    private void CM_AddOne()
    {
        if (_testItem) AddItem(_testItem, 1);
    }

    [ContextMenu("Test/Remove Selected Item (1)")]
    private void CM_RemoveOne()
    {
        if (_testItem) RemoveItemById(_testItem.id, 1);
    }

    [Header("Pruebas (Editor)")]
    public ItemData _testItem;
}
