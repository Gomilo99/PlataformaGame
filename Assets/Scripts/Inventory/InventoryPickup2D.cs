using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InventoryPickup2D : MonoBehaviour
{
    public ItemData item;
    public int amount = 1;
    [Tooltip("Etiqueta del jugador que puede recoger.")]
    public string playerTag = "Player";
    [Tooltip("Destruir este objeto al recoger con éxito.")]
    public bool destroyOnPickup = true;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (item == null)
        {
            Debug.LogWarning($"Pickup {name}: item no asignado");
            return;
        }
        if (InventoryRuntime.Instance == null || InventoryRuntime.Instance.model == null)
        {
            Debug.LogWarning("Pickup: InventoryRuntime o model no asignados en escena.");
            return;
        }
        bool ok = InventoryRuntime.Instance.AddItem(item, amount);
        if (ok)
        {
            if (destroyOnPickup) Destroy(gameObject);
        }
        else
        {
            // Inventario lleno o no se pudo apilar; puedes reproducir feedback.
            Debug.Log("Inventario lleno o no se pudo añadir.");
        }
    }
}
