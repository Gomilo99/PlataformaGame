using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    public enum PositionMode { AtMouseClamped, FixedAnchored, KeepCurrent }
    [Header("Posicionamiento")]
    [Tooltip("AtMouseClamped: cerca del cursor y dentro del Canvas. FixedAnchored: usa una posición fija (anchored). KeepCurrent: no mueve la posición, solo muestra/oculta y actualiza contenido.")]
    public PositionMode positionMode = PositionMode.AtMouseClamped;
    [Tooltip("Solo AtMouseClamped: desplazamiento respecto al cursor")] public Vector2 offset = new Vector2(10f, -10f);
    [Tooltip("Solo AtMouseClamped: margen respecto a los bordes del Canvas")] public float screenMargin = 8f;
    [Tooltip("Solo FixedAnchored: posición anclada fija en coordenadas locales del Canvas")] public Vector2 fixedAnchoredPosition = Vector2.zero;

    private InventoryUI _ui;
    private int _slotIndex = -1;
    private ItemData _item;
    [Header("Cierre por clic fuera")]
    [Tooltip("Cerrar al hacer clic fuera del panel")] public bool closeOnOutsideClick = true;
    [Tooltip("Usar GraphicRaycaster para comprobar si el clic fue sobre el panel (recomendado)")] public bool useGraphicRaycastForOutside = true;
    [Tooltip("Ignora el clic que abre el menú hasta que se suelte el botón del mouse")] public bool ignoreOpeningClick = true;
    private bool _suppressOutsideUntilPointerUp = false;

    private void Awake()
    {
        Hide();
        if (closeButton) closeButton.onClick.AddListener(Hide);
    }

    private void Update()
    {
        // Cerrar si hay click fuera del panel
        if (panelRoot != null && panelRoot.gameObject.activeSelf && closeOnOutsideClick)
        {
            if (ignoreOpeningClick && _suppressOutsideUntilPointerUp)
            {
                // Esperar a que se suelte el botón antes de comenzar a cerrar por clic fuera
                if (Input.GetMouseButtonUp(0)) _suppressOutsideUntilPointerUp = false;
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                bool clickInside = false;

                if (useGraphicRaycastForOutside && parentCanvas)
                {
                    var raycaster = parentCanvas.GetComponent<GraphicRaycaster>();
                    if (raycaster != null)
                    {
                        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                        var results = new List<RaycastResult>();
                        raycaster.Raycast(ped, results);
                        for (int i = 0; i < results.Count; i++)
                        {
                            var t = results[i].gameObject.transform;
                            if (t == panelRoot || t.IsChildOf(panelRoot))
                            {
                                clickInside = true;
                                break;
                            }
                        }
                    }
                }
                
                // Fallback geométrico (además del raycast) para cubrir zonas sin Raycast Target
                if (!clickInside)
                {
                    // Fallback geométrico
                    if (RectTransformUtility.RectangleContainsScreenPoint(panelRoot, Input.mousePosition, parentCanvas ? (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera) : null))
                    {
                        clickInside = true;
                    }
                }

                if (!clickInside)
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

        // Posicionar panel (según modo)
        if (panelRoot)
        {
            panelRoot.gameObject.SetActive(true);
            if (ignoreOpeningClick)
            {
                // Evita que el mismo clic que abrió el menú provoque el cierre por "clic fuera"
                _suppressOutsideUntilPointerUp = true;
            }
            var canvasRT = parentCanvas ? parentCanvas.transform as RectTransform : null;
            if (positionMode == PositionMode.AtMouseClamped)
            {
                if (canvasRT == null)
                {
                    Debug.LogWarning("InventoryContextMenu: parentCanvas no asignado o no es RectTransform.");
                    return;
                }
                // Convertir Screen → Local del Canvas y aplicar offset
                if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenPos, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera, out var localPoint))
                {
                    localPoint = Vector2.zero;
                }
                Vector2 desired = localPoint + offset;

                // Clampear dentro del Canvas
                Vector2 panelSize = panelRoot.rect.size;
                Vector2 canvasSize = canvasRT.rect.size;
                float minX = -canvasSize.x * 0.5f + screenMargin + panelSize.x * panelRoot.pivot.x;
                float maxX =  canvasSize.x * 0.5f - screenMargin - panelSize.x * (1f - panelRoot.pivot.x);
                float minY = -canvasSize.y * 0.5f + screenMargin + panelSize.y * (1f - panelRoot.pivot.y);
                float maxY =  canvasSize.y * 0.5f - screenMargin - panelSize.y * panelRoot.pivot.y;
                desired.x = Mathf.Clamp(desired.x, minX, maxX);
                desired.y = Mathf.Clamp(desired.y, minY, maxY);
                panelRoot.anchoredPosition = desired;
            }
            else if (positionMode == PositionMode.FixedAnchored)
            {
                panelRoot.anchoredPosition = fixedAnchoredPosition;
            }
            else // KeepCurrent
            {
                // No modificar anchoredPosition: mantiene la posición que tenga en el editor/último estado
            }
        }
    }

    public void Hide()
    {
        if (panelRoot) panelRoot.gameObject.SetActive(false);
    }
}
