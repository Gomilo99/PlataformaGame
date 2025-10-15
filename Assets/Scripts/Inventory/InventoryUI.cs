using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI simple para inventario: crea botones por slot, muestra icono y cantidad, y reacciona a hover/click.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public InventoryModel model;
    public Transform gridRoot; // Contenedor con GridLayoutGroup
    public GameObject slotPrefab; // Debe contener Image (icono) + Text/ TMP para cantidad

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
        if (!model || !gridRoot || !slotPrefab) return;
        foreach (Transform child in gridRoot) Destroy(child.gameObject);

        for (int i = 0; i < model.slots.Count; i++)
        {
            var s = model.slots[i];
            var go = Instantiate(slotPrefab, gridRoot);
            var icon = go.GetComponentInChildren<Image>();
            var texts = go.GetComponentsInChildren<Text>();
            var countLabel = texts != null && texts.Length > 0 ? texts[0] : null;

            if (s.item != null)
            {
                if (icon) { icon.enabled = true; icon.sprite = s.item.icon; }
                if (countLabel) countLabel.text = s.count > 1 ? s.count.ToString() : "";
            }
            else
            {
                if (icon) { icon.enabled = false; icon.sprite = null; }
                if (countLabel) countLabel.text = "";
            }

            // Interacción con EventSystem
            var slotInteract = go.AddComponent<InventoryUISlot>();
            slotInteract.Setup(this, i);
        }
    }

    public void OnSlotClicked(int index, PointerEventData.InputButton button)
    {
        // Ejemplo: botón derecho para desequipar/usar
        // Aquí puedes abrir tooltip, arrastrar, usar, etc.
        Debug.Log($"Slot {index} click {button}");
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
        ui?.OnSlotClicked(index, eventData.button);
    }
}
