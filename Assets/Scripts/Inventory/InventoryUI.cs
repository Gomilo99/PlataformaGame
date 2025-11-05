using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI simple para inventario: crea botones por slot, muestra icono y cantidad, y reacciona a hover/click.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public InventoryModel model;
    public Transform gridRoot; // Contenedor con GridLayoutGroup
    public GameObject slotPrefab; // Debe contener Image (icono) + Text/ TMP para cantidad
    [Header("UI - Menú contextual")]
    public global::InventoryContextMenu contextMenu; // Asignar en inspector
    [Header("Referencias de Juego")]
    public global::PlayerStats playerStats; // Opcional: para aplicar consumibles/equipar armas

    private void OnEnable()
    {
        if (model) model.OnChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (model) model.OnChanged -= Refresh;
    }

    public void Refresh()
    {
        if (!model || !gridRoot || !slotPrefab)
        {
            Debug.LogWarning("InventoryUI: Faltan referencias (model/gridRoot/slotPrefab)");
            return;
        }
        if (gridRoot.gameObject == slotPrefab)
        {
            Debug.LogError("InventoryUI: No asignes el slotPrefab en gridRoot. gridRoot debe ser el contenedor (Content) y slotPrefab un prefab de elemento.");
            return;
        }
    foreach (Transform child in gridRoot) Destroy(child.gameObject);

        for (int i = 0; i < model.slots.Count; i++)
        {
            var s = model.slots[i];
            var go = Instantiate(slotPrefab, gridRoot);
            // Buscar hijos por nombre: "icono" (Image), "texto" (TMP nombre) y opcional "cantidad" (TMP cantidad)
            Image icon = null;
            TextMeshProUGUI label = null;
            TextMeshProUGUI countLabel = null;
            var iconT = go.transform.Find("icono");
            if (iconT) icon = iconT.GetComponent<Image>();
            var textT = go.transform.Find("texto");
            if (textT) label = textT.GetComponent<TextMeshProUGUI>();
            var countT = go.transform.Find("cantidad");
            if (countT) countLabel = countT.GetComponent<TextMeshProUGUI>();

            if (s.item != null)
            {
                if (icon) { icon.enabled = true; icon.sprite = s.item.icon; }
                if (countLabel != null)
                {
                    if (label) label.text = s.item.displayName;
                    countLabel.text = s.count > 1 ? $"x{s.count}" : string.Empty;
                }
                else
                {
                    // Fallback: sin campo "cantidad", mostrar nombre + cantidad en el mismo texto
                    if (label) { label.text = s.count > 1 ? $"{s.item.displayName} x{s.count}" : s.item.displayName; }
                }
            }
            else
            {
                if (icon) { icon.enabled = false; icon.sprite = null; }
                if (label) label.text = string.Empty;
                if (countLabel) countLabel.text = string.Empty;
            }

            // Interacción con EventSystem
            var slotInteract = go.AddComponent<InventoryUISlot>();
            slotInteract.Setup(this, i);
        }
    }

    public void OnSlotClicked(int index, PointerEventData eventData)
    {
        var slot = (index >= 0 && index < model.slots.Count) ? model.slots[index] : null;
        if (slot == null) return;
        // Solo abrir menú si hay ítem
        if (slot.item == null) return;

        if (contextMenu)
        {
            contextMenu.Show(this, index, slot.item, slot.count, eventData.position);
        }
        else
        {
            Debug.Log($"Slot {index} click {eventData.button}");
        }
    }

    // Acciones desde el menú
    public void Consume(int index)
    {
        var slot = model.GetSlot(index);
        if (slot == null || slot.item == null) return;
        if (slot.item.category != ItemCategory.Consumable) return;

        if (playerStats)
        {
            playerStats.ApplyConsumable(slot.item);
        }
        // Reducir stack en 1
        model.RemoveAt(index, 1);
    }

    public void Equip(int index)
    {
        var slot = model.GetSlot(index);
        if (slot == null || slot.item == null) return;
        if (slot.item.category != ItemCategory.Weapon) return;
        if (playerStats)
        {
            playerStats.EquipWeapon(slot.item);
        }
    }

    public void Drop(int index)
    {
        var slot = model.GetSlot(index);
        if (slot == null || slot.item == null) return;
        if (slot.item.category == ItemCategory.Key) return; // no botar claves
        // Remover 1 del stack, si stack <=1 vacía el slot
        model.RemoveAt(index, 1);
    }
}

public class InventoryUISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private InventoryUI ui;
    private int index;
    private Image bg;
    private Color norm = Color.white;
    private Color hover = new Color(0.9f, 0.95f, 1f, 1f);

    public void Setup(InventoryUI ui, int index)
    {
        this.ui = ui;
        this.index = index;
        bg = GetComponent<Image>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (bg) bg.color = hover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (bg) bg.color = norm;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ui?.OnSlotClicked(index, eventData);
    }
}
