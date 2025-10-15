using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class ParallaxController : MonoBehaviour
{
    public enum UpdatePhase { Update, LateUpdate, FixedUpdate }
    [Header("Target de referencia")]
    [Tooltip("Transform que se usará como referencia para el parallax (cámara o jugador). Por defecto, la cámara principal.")]
    public Transform target;

    [Header("Parallax")]
    [Tooltip("Factor de parallax horizontal (0 = fijo, 1 = se mueve igual que el target)")]
    public float parallaxEffectX = 0.5f;
    [Tooltip("Habilitar movimiento vertical por parallax")]
    public bool enableVerticalParallax = false;
    [Tooltip("Factor de parallax vertical (0 = fijo, 1 = se mueve igual que el target)")]
    public float parallaxEffectY = 0.2f;

    [Header("Ajustes verticales avanzados")]
    [Tooltip("Aplicar un sesgo hacia abajo cuando el target asciende. Útil para que capas frontales queden visualmente más abajo al subir.")]
    public bool enableVerticalBias = false;
    [Tooltip("Factor a restar cuando el target asciende: yOffset -= max(0, deltaY) * verticalBiasOnAscent")]
    public float verticalBiasOnAscent = 0f;
    [Tooltip("Limitar el desplazamiento vertical (en unidades de mundo) relativo a la posición inicial de esta capa.")]
    public bool enableVerticalClamp = false;
    [Tooltip("Desplazamiento vertical mínimo permitido (negativo = hacia abajo) desde la posición inicial.")]
    public float minYOffset = -9999f;
    [Tooltip("Desplazamiento vertical máximo permitido (positivo = hacia arriba) desde la posición inicial.")]
    public float maxYOffset = 9999f;

    [Header("Tiling infinito")]
    [Tooltip("Repetir el sprite horizontalmente para evitar que se pierda el fondo")]
    public bool infiniteHorizontal = true;
    [Tooltip("Repetir el sprite verticalmente (si usas texturas tileables verticalmente)")]
    public bool infiniteVertical = false;

    [Header("Actualización y nitidez")]
    [Tooltip("Fase del ciclo en la que se aplicará el parallax. Usa LateUpdate para cámaras; FixedUpdate si sigues un Rigidbody sin interpolación.")]
    public UpdatePhase updatePhase = UpdatePhase.LateUpdate;
    [Tooltip("Redondear posición a la rejilla de píxeles (útil para pixel art y reducir shimmer).")]
    public bool enablePixelSnap = false;
    [Tooltip("Píxeles por unidad para el redondeo de pixel snap.")]
    public float pixelsPerUnit = 100f;

    private SpriteRenderer _sr;
    private float _lengthX;
    private float _lengthY;
    private Vector2 _startPos;
    private Vector2 _targetStartPos;

    void Awake()
    {
        if (!target && Camera.main)
            target = Camera.main.transform;
        _sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        _startPos = transform.position;
        if (target) _targetStartPos = target.position;
        if (_sr)
        {
            var bounds = _sr.bounds.size;
            _lengthX = bounds.x;
            _lengthY = bounds.y;
        }
    }

    void Update()
    {
        if (updatePhase == UpdatePhase.Update)
            ApplyParallax();
    }

    void LateUpdate()
    {
        if (updatePhase == UpdatePhase.LateUpdate)
            ApplyParallax();
    }

    void FixedUpdate()
    {
        if (updatePhase == UpdatePhase.FixedUpdate)
            ApplyParallax();
    }

    private void ApplyParallax()
    {
        if (!target) return;

        // Delta desde que empezó el juego (evita saltos al inicio)
        Vector2 delta = (Vector2)target.position - _targetStartPos;

        float newX = _startPos.x + delta.x * parallaxEffectX;

        // Cálculo del offset vertical con opciones de bias y clamp
        float yOffset = 0f;
        if (enableVerticalParallax)
        {
            yOffset = delta.y * parallaxEffectY;
            if (enableVerticalBias && verticalBiasOnAscent != 0f)
            {
                float ascent = Mathf.Max(0f, delta.y); // solo cuando sube
                yOffset -= ascent * verticalBiasOnAscent;
            }
            if (enableVerticalClamp)
            {
                yOffset = Mathf.Clamp(yOffset, minYOffset, maxYOffset);
            }
        }
        float newY = _startPos.y + yOffset;

        // Cálculo de tiling sin mutar _startPos para evitar saltos al iniciar
        float wrapX = 0f;
        float wrapY = 0f;

        if (infiniteHorizontal && _lengthX > 0.0001f)
        {
            float tempX = delta.x * (1f - parallaxEffectX);
            int tilesX = tempX >= 0f ? Mathf.FloorToInt(tempX / _lengthX) : Mathf.CeilToInt(tempX / _lengthX);
            wrapX = tilesX * _lengthX;
        }

        if (enableVerticalParallax && infiniteVertical && _lengthY > 0.0001f)
        {
            float tempY = delta.y * (1f - parallaxEffectY);
            int tilesY = tempY >= 0f ? Mathf.FloorToInt(tempY / _lengthY) : Mathf.CeilToInt(tempY / _lengthY);
            wrapY = tilesY * _lengthY;
        }

        // Aplicar posición final con parallax + wrapping
        Vector3 pos = transform.position;
        pos.x = newX + wrapX;
    pos.y = newY + wrapY;

        if (enablePixelSnap && pixelsPerUnit > 0f)
        {
            pos.x = Mathf.Round(pos.x * pixelsPerUnit) / pixelsPerUnit;
            pos.y = Mathf.Round(pos.y * pixelsPerUnit) / pixelsPerUnit;
        }

        transform.position = pos;
    }
}