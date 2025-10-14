using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Barra de vida para enemigos con "chip" gris que se retrasa al bajar.
/// Diseñada para usarse como hijo del enemigo en un Canvas World Space.
/// Requiere dos Image con tipo Filled (Horizontal o Radial 360 según prefieras).
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Referencia al script Enemigo del cual leer la vida actual.")]
    public Enemigo target;

    [Tooltip("Image del fill principal (vida actual). Debe ser Image tipo Filled.")]
    public Image fillCurrent;

    [Tooltip("Image del fill de chip (daño en gris). Debe ser Image tipo Filled.")]
    public Image fillChip;
    [Header("Comportamiento visual")]
    [Tooltip("Retraso antes de que el chip gris comience a bajar hacia el valor actual.")]
    public float chipDelay = 0.2f;
    [Tooltip("Velocidad a la que el chip gris interpola hacia el valor actual.")]
    public float chipLerpSpeed = 2.5f;
    [Tooltip("Cuando el enemigo está a vida completa, oculta la barra.")]
    public bool hideWhenFull = true;
    [Tooltip("Evitar que la barra se invierta cuando el enemigo invierte el sprite (escala X negativa).")]
    public bool keepPositiveScaleX = true;
    private float maxHealth = 0f;
    private float _lastHealth;
    private float _targetFrac;
    private float _currentFrac;
    private float _chipFrac;
    private Coroutine _chipRoutine;
    // Para detectar cambios del toggle en tiempo de ejecución y refrescar visibilidad
    private bool _lastHideWhenFull;

    private void Reset()
    {
        // Intentar autocompletar referencias cuando se agrega el componente
        if (!target) target = GetComponentInParent<Enemigo>();
        var images = GetComponentsInChildren<Image>();
        if (images != null)
        {
            if (images.Length > 0) fillCurrent = images[0];
            if (images.Length > 1) fillChip = images[1];
        }
    }

    private void Awake()
    {
        if (!target) target = GetComponentInParent<Enemigo>();
        if (target == null)
        {
            Debug.LogWarning($"{nameof(EnemyHealthBar)} en {name} no encontró Enemigo en padres.");
        }
        var canvas = GetComponent<Canvas>();
        if (canvas && Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
            
        }
        if (maxHealth <= 0f && target != null)
        {
            maxHealth = Mathf.Max(1f, target.vida);
        }

        _lastHealth = target ? target.vida : maxHealth;
        _targetFrac = _currentFrac = _chipFrac = GetFraction(_lastHealth);
        ApplyFillInstant();
        UpdateVisibility();
    _lastHideWhenFull = hideWhenFull;

        // Asegurar orden de dibujo: el fill verde arriba del chip gris
        if (fillCurrent && fillChip)
        {
            // El último hermano se dibuja encima en UI
            if (fillCurrent.transform.GetSiblingIndex() <= fillChip.transform.GetSiblingIndex())
                fillCurrent.transform.SetAsLastSibling();
        }

        // No bloquear raycasts de UI
        if (fillCurrent) fillCurrent.raycastTarget = false;
        if (fillChip) fillChip.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (target)
        {
            // Leer vida del enemigo y detectar cambios
            float hp = Mathf.Clamp(target.vida, 0f, maxHealth > 0 ? maxHealth : Mathf.Max(1f, target.vida));
            if (!Mathf.Approximately(hp, _lastHealth))
            {
                OnHealthChanged(hp, hp < _lastHealth);
                _lastHealth = hp;
            }
        }

        // Avanzar interpolación del chip si está en curso
        if (_chipRoutine == null && _chipFrac > _currentFrac)
        {
            // Seguridad adicional: seguir acercando el chip si quedó a medio camino
            _chipFrac = Mathf.MoveTowards(_chipFrac, _currentFrac, chipLerpSpeed * Time.deltaTime);
            ApplyFill();
        }

        if (keepPositiveScaleX)
        {
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x);
            transform.localScale = s;
        }

        // Si el usuario cambia hideWhenFull en tiempo de ejecución desde el Inspector, refrescar visibilidad
        if (_lastHideWhenFull != hideWhenFull)
        {
            _lastHideWhenFull = hideWhenFull;
            UpdateVisibility();
        }
    }

    private void OnHealthChanged(float newHealth, bool isDamage)
    {
        _targetFrac = GetFraction(newHealth);

        if (isDamage)
        {
            // La barra principal cae de inmediato al nuevo valor.
            _currentFrac = _targetFrac;
            // El chip se retrasa y luego cae suavemente hasta el valor actual.
            if (_chipRoutine != null) StopCoroutine(_chipRoutine);
            _chipRoutine = StartCoroutine(AnimateChipDown());
        }
        else
        {
            // Curación: llevamos ambos al nuevo valor (sin destacar daño)
            _currentFrac = _targetFrac;
            _chipFrac = _targetFrac;
        }

        ApplyFill();
        UpdateVisibility();
    }

    private IEnumerator AnimateChipDown()
    {
        // Espera el retraso antes de bajar el chip
        if (chipDelay > 0f) yield return new WaitForSeconds(chipDelay);
        while (_chipFrac > _currentFrac)
        {
            _chipFrac = Mathf.MoveTowards(_chipFrac, _currentFrac, chipLerpSpeed * Time.deltaTime);
            ApplyFill();
            yield return null;
        }
        _chipFrac = _currentFrac;
        ApplyFill();
        _chipRoutine = null;
    }

    private float GetFraction(float health)
    {
        float max = Mathf.Max(1f, maxHealth);
        return Mathf.Clamp01(health / max);
    }

    private void ApplyFillInstant()
    {
        if (fillCurrent) fillCurrent.fillAmount = _currentFrac;
        if (fillChip) fillChip.fillAmount = _chipFrac;
    }

    private void ApplyFill()
    {
        if (fillCurrent && !Mathf.Approximately(fillCurrent.fillAmount, _currentFrac))
            fillCurrent.fillAmount = _currentFrac;
        if (fillChip && !Mathf.Approximately(fillChip.fillAmount, _chipFrac))
            fillChip.fillAmount = _chipFrac;
    }

    private void UpdateVisibility()
    {
        // Mostrar siempre si hideWhenFull es false; de lo contrario, ocultar solo cuando está ~al 100%
        bool show = !hideWhenFull || _currentFrac < 0.999f;
        if (fillCurrent) fillCurrent.enabled = show;
        if (fillChip) fillChip.enabled = show;
    }
}
