using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Comportamiento de botón que reacciona a hover y click usando interfaces de Unity.
/// Cambia colores/escala opcionalmente y expone eventos UnityEvent.
/// </summary>
public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Visuales")]
    public Graphic targetGraphic; // Image o Text
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.9f, 0.9f, 1f, 1f);
    public Color pressedColor = new Color(0.8f, 0.8f, 1f, 1f);
    public bool scaleOnHover = true;
    public float hoverScale = 1.05f;

    private Vector3 _originalScale;

    private void Awake()
    {
        _originalScale = transform.localScale;
        if (!targetGraphic) targetGraphic = GetComponent<Graphic>();
        SetColor(normalColor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetColor(hoverColor);
        if (scaleOnHover) transform.localScale = _originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetColor(normalColor);
        if (scaleOnHover) transform.localScale = _originalScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetColor(pressedColor);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetColor(hoverColor);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Acciones las maneja el Button o UnityEvents en el inspector.
    }

    private void SetColor(Color c)
    {
        if (targetGraphic) targetGraphic.color = c;
    }
}
