using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventoryContextMenu : MonoBehaviour
{
    [Header("Referencias UI")]
    public RectTransform panelRoot; // Panel principal a posicionar/mostrar
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI descriptionLabel;
    public Button consumeButton;
    public Button equipButton;
    public Button dropButton;
    public Button closeButton;
    public Canvas parentCanvas; // Canvas para convertir posiciones de pantalla a UI

    private InventoryUI _ui;
    private int _slotIndex = -1;
    private ItemData _item;

    private void Awake()
    {
        Hide();
        if (closeButton) closeButton.onClick.AddListener(Hide);
    }

    private void Update()
    {
        // Cerrar si hay click fuera del panel
        if (panelRoot != null && panelRoot.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(panelRoot, Input.mousePosition, parentCanvas ? parentCanvas.worldCamera : null))
                {
                    Hide();
                }
            }
        }
    }

    public void Show(InventoryUI ui, int slotIndex, ItemData item, int count, Vector2 screenPos)
    {
        _ui = ui; _slotIndex = slotIndex; _item = item;
        if (titleLabel) titleLabel.text = item != null ? item.displayName : "";
        if (descriptionLabel) descriptionLabel.text = item != null ? item.description : "";

        // Configurar botones según categoría de ítem
        bool isConsumable = item != null && item.category == ItemCategory.Consumable;
        bool isWeapon = item != null && item.category == ItemCategory.Weapon;
        bool isKey = item != null && item.category == ItemCategory.Key;

        if (consumeButton)
        {
            consumeButton.gameObject.SetActive(isConsumable);
            consumeButton.onClick.RemoveAllListeners();
            if (isConsumable) consumeButton.onClick.AddListener(() => { _ui?.Consume(_slotIndex); Hide(); });
        }
        if (equipButton)
        {
            equipButton.gameObject.SetActive(isWeapon);
            equipButton.onClick.RemoveAllListeners();
            if (isWeapon) equipButton.onClick.AddListener(() => { _ui?.Equip(_slotIndex); Hide(); });
        }
        if (dropButton)
        {
            // Claves no se pueden botar
            bool canDrop = !isKey;
            dropButton.gameObject.SetActive(canDrop);
            dropButton.onClick.RemoveAllListeners();
            if (canDrop) dropButton.onClick.AddListener(() => { _ui?.Drop(_slotIndex); Hide(); });
        }

        // Posicionar panel
        if (panelRoot)
        {
            panelRoot.gameObject.SetActive(true);
            Vector2 uiPos = screenPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas ? parentCanvas.worldCamera : null, out uiPos);
            panelRoot.anchoredPosition = uiPos;
        }
    }

    public void Hide()
    {
        if (panelRoot) panelRoot.gameObject.SetActive(false);
    }
}
