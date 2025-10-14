using System.Collections;
using UnityEngine;

/// <summary>
/// Comportamiento de persecución simple: si el jugador entra en radio de detección,
/// se suspende la patrulla y el enemigo se mueve hacia el jugador. Si sale del radio + hysteresis, vuelve a patrullar.
/// Depende de un Rigidbody2D y opcionalmente de EnemyPatrol2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(EnemyPatrol2D), typeof(Animator))]
public class EnemyChaseBehaviour : MonoBehaviour
{
    [Header("Detección Base / Caja (Rectángulo)")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private Transform player;
    [Tooltip("Ancho de la caja de detección (X)")]
    [SerializeField] private float boxWidth = 6f;
    [Tooltip("Alto de la caja de detección (Y)")]
    [SerializeField] private float boxHeight = 2.5f;
    [Tooltip("Expansión para el área de pérdida (hysteresis)")]
    [SerializeField] private float loseBoxExpand = 1.0f;
    [Tooltip("Offset horizontal de la caja de detección, positivo mira hacia delante")]
    [SerializeField] private float detectOffsetX = 0f;
    [Tooltip("Offset vertical de la caja de detección")]
    [SerializeField] private float detectOffsetY = 0f;

    [Header("Movimiento persecución")]
    [SerializeField] private float chaseSpeedMultiplier = 1.2f;
    [Tooltip("Diferencia vertical permitida para iniciar/continuar persecución")]
    [SerializeField] private float verticalTolerance = 1.5f;
    [Tooltip("Offset vertical aplicado al centro para evaluar la tolerancia vertical de persecución")]
    [SerializeField] private float verticalCenterOffset = 0f;
    [Tooltip("Permite activar/desactivar la restricción de tolerancia vertical para la persecución")]
    [SerializeField] private bool enableVerticalTolerance = true;

    [Header("Ataque")]
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private AudioClip attackSound;
    [Tooltip("Ventana vertical para permitir ataque (círculo de rango se mantiene)")]
    [SerializeField] private float attackVerticalWindow = 1.2f;
    [Tooltip("Offset vertical adicional para evaluar la ventana de ataque")]
    [SerializeField] private float attackVerticalCenterOffset = 0f;
    [Tooltip("Tiempo fijo tras disparar el ataque antes de liberar el estado de ataque")]

    [Header("Refs")]
    [SerializeField] private EnemyPatrol2D patrol;
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private RigidbodyConstraints2D savedConstraints;
    private float baseSpeed;
    private bool chasing;
    private float nextAttackTime;
    private bool isAttacking;
    // Sin radios; usamos expansión rectangular para hysteresis

    [Header("Gizmos (Detección)")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoDetectColor = new Color(0f, 1f, 0.25f, 0.6f);
    [SerializeField] private Color gizmoLoseColor = new Color(1f, 0.5f, 0f, 0.45f);
    [SerializeField] private Color gizmoAttackRangeColor = new Color(1f, 0.95f, 0f, 0.9f);
    [SerializeField] private Color gizmoVertTolColor = new Color(0f, 0.8f, 1f, 0.85f);
    [SerializeField] private Color gizmoAttackWindowColor = new Color(1f, 0f, 1f, 0.85f);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        patrol = GetComponent<EnemyPatrol2D>();
        baseSpeed = patrol.BaseSpeed;
        // Guardar constraints iniciales para restaurarlas después del ataque
        savedConstraints = rb.constraints;
    }

    void Start()
    {
        if (!player)
        {
            var plyGO = GameManager.Instance.player;
            if (plyGO) player = plyGO.transform;
        }
    }

    void Update()
    {
    // Distancia lineal para ataque (círculo)
        float dist = Vector2.Distance(transform.position, player.position);

        // Determinar área de detección y área de pérdida (hysteresis)
        bool inDetectArea;
        bool inLoseArea;

        int facingSign = transform.localScale.x > 0 ? 1 : -1;
        Vector2 center = (Vector2)transform.position + new Vector2(detectOffsetX * facingSign, detectOffsetY);
        Vector2 size = new Vector2(Mathf.Max(0.01f, boxWidth), Mathf.Max(0.01f, boxHeight));
        inDetectArea = Physics2D.OverlapBox(center, size, 0f, playerMask) != null;

        float ex = Mathf.Max(0f, loseBoxExpand);
        Vector2 sizeLose = new Vector2(size.x + 2f * ex, size.y + 2f * ex);
        inLoseArea = Physics2D.OverlapBox(center, sizeLose, 0f, playerMask) != null;

        // TRANSICIÓN A CHASING
        if (!chasing)
        {
            bool verticalOk = !enableVerticalTolerance || Mathf.Abs(player.position.y - (transform.position.y + verticalCenterOffset)) <= verticalTolerance;
            if (inDetectArea && verticalOk)
            {
                chasing = true;
                // Deshabilitar la patrulla para que no sobreescriba rb.velocity
                if (patrol) patrol.enabled = false;
            }
        }
        else
        {
            bool verticalOk = !enableVerticalTolerance || Mathf.Abs(player.position.y - (transform.position.y + verticalCenterOffset)) <= verticalTolerance;
            if (!inLoseArea || !verticalOk)
            {
                chasing = false;
                if (patrol) patrol.enabled = true; // reactivar patrulla al salir del chase
            }
        }

        if (chasing)
        {
            if (!isAttacking)
            {
                IntentarAtacar(dist);
                Perseguir();
            }
        }
    }

    [ContextMenu("Perseguir")]
    private void Perseguir()
    {
        if (!player) return;
        // Dirección horizontal con signo (-1 izquierda, +1 derecha)
        float dx = player.position.x - transform.position.x;
        float dirX = Mathf.Abs(dx) > 0.001f ? Mathf.Sign(dx) : 0f;
        Vector2 dir = new Vector2(dirX, 0f);

        float effectiveSpeed = baseSpeed > 0 ? baseSpeed * chaseSpeedMultiplier : 2f * chaseSpeedMultiplier;
        rb.velocity = new Vector2(dir.x * effectiveSpeed, rb.velocity.y);

        animator.SetBool(Enemigo.isRunning, true);
        // Flip manual
        if (Mathf.Abs(dir.x) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = dir.x > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }
    private void IntentarAtacar(float currentDistance)
    {
        if (currentDistance > attackRange) return;
        if (Time.time < nextAttackTime) return;
        if (Mathf.Abs(player.position.y - (transform.position.y + attackVerticalCenterOffset)) > attackVerticalWindow) return;
        Debug.Log("Intentando atacar");
        isAttacking = true;
        rb.velocity = new Vector2(0, rb.velocity.y);

        // Durante el ataque bloqueamos la posición X para evitar que fuerzas externas (p.ej. empujes del jugador)
        // hagan que el enemigo salga "volando" cuando su velocidad se pone a 0.
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        animator.SetBool(Enemigo.isRunning, false);
    // La patrulla ya está deshabilitada durante el chase

        nextAttackTime = Time.time + attackCooldown;
        animator.SetTrigger(Enemigo.attackingTrigger);
        AudioManager.Instance.ReproducirSonido(attackSound);
        
        // Esperar a que concluya la animación de ataque o agotar timeout
        StartCoroutine(WaitTimeAttack());
    }
    private IEnumerator WaitTimeAttack()
    {
        // Esperar un frame para que el trigger cambie efectivamente el estado
        yield return null;
        // Duración efectiva del estado actual
        var info = animator.GetCurrentAnimatorStateInfo(0);
        float clipLength = info.length; // segundos

        yield return new WaitForSeconds(clipLength);

        animator.ResetTrigger(Enemigo.attackingTrigger);
        isAttacking = false;
        // Restaurar constraints originales
        rb.constraints = savedConstraints;
        // No reactivar patrulla aquí: se reactiva al salir del chase
        // Si seguimos en chase, mantenemos el candado; si no, lo liberamos en la transición de salida
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        // Rectángulos de detección y pérdida
        {
            int facingSign = transform.localScale.x > 0 ? 1 : -1;
            Vector2 center = (Vector2)transform.position + new Vector2(detectOffsetX * facingSign, detectOffsetY);
            Vector2 size = new Vector2(Mathf.Max(0.01f, boxWidth), Mathf.Max(0.01f, boxHeight));
            // Detección
            Gizmos.color = gizmoDetectColor;
            DrawWireRect(center, size);
            // Pérdida
            float ex = Mathf.Max(0f, loseBoxExpand);
            Vector2 sizeLose = new Vector2(size.x + 2f * ex, size.y + 2f * ex);
            Gizmos.color = gizmoLoseColor;
            DrawWireRect(center, sizeLose);
        }
        // Attack range
        Gizmos.color = gizmoAttackRangeColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Bandas horizontales: tolerancia vertical y ventana de ataque
        float y0 = transform.position.y;
        if (enableVerticalTolerance)
        {
            DrawHorizontalBand(y0 + verticalCenterOffset - verticalTolerance, y0 + verticalCenterOffset + verticalTolerance, gizmoVertTolColor);
        }
        DrawHorizontalBand(y0 + attackVerticalCenterOffset - attackVerticalWindow, y0 + attackVerticalCenterOffset + attackVerticalWindow, gizmoAttackWindowColor);
    }

    private void DrawHorizontalBand(float yMin, float yMax, Color c)
    {
        Gizmos.color = c;
        float span = Mathf.Max(boxWidth * 0.5f, 1.0f) * 1.5f;
        Vector3 left1 = new Vector3(transform.position.x - span, yMin, 0f);
        Vector3 right1 = new Vector3(transform.position.x + span, yMin, 0f);
        Vector3 left2 = new Vector3(transform.position.x - span, yMax, 0f);
        Vector3 right2 = new Vector3(transform.position.x + span, yMax, 0f);
        Gizmos.DrawLine(left1, right1);
        Gizmos.DrawLine(left2, right2);
#if UNITY_EDITOR
        // Etiquetas
        UnityEditor.Handles.color = c;
        UnityEditor.Handles.Label((left1 + right1) * 0.5f, $"y={yMin:F2}");
        UnityEditor.Handles.Label((left2 + right2) * 0.5f, $"y={yMax:F2}");
#endif
    }

    private void DrawWireRect(Vector2 center, Vector2 size)
    {
        Vector3 half = (Vector3)(size * 0.5f);
        Vector3 bl = new Vector3(center.x - half.x, center.y - half.y, 0f);
        Vector3 br = new Vector3(center.x + half.x, center.y - half.y, 0f);
        Vector3 tr = new Vector3(center.x + half.x, center.y + half.y, 0f);
        Vector3 tl = new Vector3(center.x - half.x, center.y + half.y, 0f);
        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
#if UNITY_EDITOR
        // Opcional: rellenar muy tenue
        UnityEditor.Handles.DrawSolidRectangleWithOutline(new Vector3[] { bl, br, tr, tl }, new Color(0, 0, 0, 0), Gizmos.color);
#endif
    }
}
