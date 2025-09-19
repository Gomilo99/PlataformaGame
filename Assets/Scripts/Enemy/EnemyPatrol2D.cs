using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol2D : MonoBehaviour
{
    public enum PatrolMode
    {
        BetweenPoints, // Se mueve entre A y B
        Range,         // Se mueve entre centerX - leftOffset y centerX + rightOffset
        GroundWalker   // "Goomba": camina y gira si no hay suelo delante o hay pared
    }

    [Header("Modo de patrulla")]
    [SerializeField] private PatrolMode mode = PatrolMode.GroundWalker;

    //[Header("Movimiento básico")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool startFacingRight = false;
    [SerializeField] private float waitAtTurn = 0f; // pausa al girar

    //[Header("Entre puntos")]
    [SerializeField] private Transform pointA;

    [SerializeField] private Transform pointB;

    [SerializeField] private float arriveThreshold = 0.05f;

    //[Header("Por rango")]
    [SerializeField] private Transform center; // si es null se usa la X actual al iniciar

    [SerializeField] private float leftOffset = 2f;

    [SerializeField] private float rightOffset = 2f;
    [SerializeField] private bool useOwnCenter = true; // usar transform.x inicial como centro lógico (ignora 'center')
    [SerializeField] private bool useColliderWidth = false; // ajusta los límites usando el ancho del collider

    //[Header("Caminante de borde (GroundWalker)")]
    [SerializeField] private Transform groundCheck; // punto desde el que raycastear suelo

    [SerializeField] private Transform wallCheck;   // punto desde el que raycastear paredes

    [SerializeField] private LayerMask groundMask;

    [SerializeField] private float groundCheckDistance = 0.25f;

    [SerializeField] private float wallCheckDistance = 0.1f;

    //[Header("Animator (opcional)")]
    [SerializeField] private Animator animator; // si se asigna, se actualizará un bool de correr
    [SerializeField] private string runningId = "isRunningRigth"; // coincide con tu Enemigo

    //[Header("Debug / Gizmos")]
    [SerializeField] private Color gizmoPatrolColor = new Color(0f, 1f, 1f, 1f); // cyan
    [SerializeField] private Color gizmoLimitColor = new Color(1f, 0f, 1f, 1f); // magenta
    [SerializeField] private Color gizmoRayColor = new Color(1f, 0.92f, 0.016f, 1f); // yellow

    private Rigidbody2D rb;
    private Collider2D col; // para calcular extents.x si se desea
    private bool facingRight;
    private bool canMove = true;
    private float centerX;
    private float originalScaleX;
    private float nextMoveTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (!col) col = GetComponentInChildren<Collider2D>();
        if (!animator) animator = GetComponent<Animator>();
        facingRight = startFacingRight;
        originalScaleX = Mathf.Abs(transform.localScale.x) < Mathf.Epsilon ? 1f : Mathf.Abs(transform.localScale.x);
    }

    void Start()
    {
        // Si se usa centro propio o no hay center asignado, tomar la X actual del enemigo
        if (mode == PatrolMode.Range)
        {
            if (useOwnCenter || !center)
                centerX = transform.position.x;
            else
                centerX = center.position.x;
        }
        else
        {
            centerX = center ? center.position.x : transform.position.x;
        }

        // Opcional: evitar que rote por físicas
        rb.freezeRotation = true;

        // Asegura orientación inicial
        ApplyFacingToScale();
    }

    void Update()
    {
        // Animator opcional
        if (animator && !string.IsNullOrEmpty(runningId))
        {
            bool isMoving = Mathf.Abs(rb.velocity.x) > 0.01f && canMove;
            animator.SetBool(runningId, isMoving);
        }
    }

    void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        if (Time.time < nextMoveTime)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        switch (mode)
        {
            case PatrolMode.BetweenPoints:
                PatrolBetweenPoints();
                break;
            case PatrolMode.Range:
                PatrolRange();
                break;
            case PatrolMode.GroundWalker:
                PatrolGroundWalker();
                break;
        }
    }

    private void PatrolBetweenPoints()
    {
        if (!pointA || !pointB)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        // Determina objetivo actual según facing
        Vector3 target = facingRight ? pointB.position : pointA.position;
        float dir = Mathf.Sign(target.x - transform.position.x);

        // Mueve
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);

        // ¿Llegó?
        if (Mathf.Abs(transform.position.x - target.x) <= arriveThreshold)
        {
            StartCoroutine(TurnAfterWait());
        }

        // Actualiza flip
        if ((dir > 0 && !facingRight) || (dir < 0 && facingRight))
            ApplyFacingToScale();
    }

    private void PatrolRange()
    {
        float leftBound = centerX - leftOffset;
        float rightBound = centerX + rightOffset;

        // Extensión horizontal del collider para considerar el borde delantero del enemigo
        float extX = (useColliderWidth && col != null) ? col.bounds.extents.x : 0f;

        // Si toca límite, girar con pausa
        if (facingRight && (transform.position.x + extX) >= rightBound)
        {
            StartCoroutine(TurnAfterWait());
        }
        else if (!facingRight && (transform.position.x - extX) <= leftBound)
        {
            StartCoroutine(TurnAfterWait());
        }

        float dir = facingRight ? 1f : -1f;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);
        ApplyFacingToScale();
    }

    private void PatrolGroundWalker()
    {
        // Raycast al suelo delante
        bool groundAhead = true;
        if (groundCheck)
        {
            Vector2 gcOrigin = groundCheck.position;
            Vector2 gcDir = Vector2.down;
            groundAhead = Physics2D.Raycast(gcOrigin, gcDir, groundCheckDistance, groundMask);
        }

        // Raycast a pared delante
        bool wallAhead = false;
        if (wallCheck)
        {
            Vector2 wcOrigin = wallCheck.position;
            Vector2 wcDir = facingRight ? Vector2.right : Vector2.left;
            wallAhead = Physics2D.Raycast(wcOrigin, wcDir, wallCheckDistance, groundMask);
        }

        if (!groundAhead || wallAhead)
        {
            StartCoroutine(TurnAfterWait());
        }

        float dir = facingRight ? 1f : -1f;
        rb.velocity = new Vector2(dir * speed, rb.velocity.y);
        ApplyFacingToScale();
    }

    private IEnumerator TurnAfterWait()
    {
        if (!canMove) yield break;

        canMove = false;
        float pause = Mathf.Max(0f, waitAtTurn);
        if (pause > 0f)
            yield return new WaitForSeconds(pause);

        facingRight = !facingRight;
        ApplyFacingToScale();

        canMove = true;
        // Evita superar el límite por una física tardía
        nextMoveTime = Time.time + 0.01f;
    }

    private void ApplyFacingToScale()
    {
        Vector3 s = transform.localScale;
        s.x = (facingRight ? 1f : -1f) * originalScaleX;
        transform.localScale = s;
    }

    // API pública mínima
    public void Pause() => canMove = false;
    public void Resume() => canMove = true;
    public void SetFacingRight(bool value)
    {
        facingRight = value;
        ApplyFacingToScale();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoPatrolColor;
        if (mode == PatrolMode.BetweenPoints)
        {
            if (pointA) Gizmos.DrawWireSphere(pointA.position, 0.1f);
            if (pointB) Gizmos.DrawWireSphere(pointB.position, 0.1f);
            if (pointA && pointB) Gizmos.DrawLine(pointA.position, pointB.position);
        }
        else if (mode == PatrolMode.Range)
        {
            // Si se eligió usar el centro propio, ignorar 'center' incluso en editor
            float c;
            if (useOwnCenter)
            {
                c = Application.isPlaying ? centerX : transform.position.x;
            }
            else
            {
                c = center ? center.position.x : (Application.isPlaying ? centerX : transform.position.x);
            }
            Vector3 left = new Vector3(c - leftOffset, transform.position.y, 0);
            Vector3 right = new Vector3(c + rightOffset, transform.position.y, 0);
            Gizmos.DrawLine(left, right);
            // puntos de límite con color diferente
            Gizmos.color = gizmoLimitColor;
            Gizmos.DrawWireSphere(left, 0.08f);
            Gizmos.DrawWireSphere(right, 0.08f);
            Gizmos.color = gizmoPatrolColor;

            // Si se usa el ancho del collider, dibujar una guía del extents en el centro actual
            if (useColliderWidth && col != null)
            {
                float extX = col.bounds.extents.x;
                Vector3 pos = Application.isPlaying ? transform.position : (Application.isPlaying ? transform.position : transform.position);
                // Líneas pequeñas que marcan el borde delantero y trasero basado en facing (en editor asumimos derecha)
                Gizmos.color = new Color(0.3f, 1f, 0.3f, 0.9f);
                Gizmos.DrawLine(new Vector3(left.x, pos.y - 0.1f, 0), new Vector3(left.x, pos.y + 0.1f, 0));
                Gizmos.DrawLine(new Vector3(right.x, pos.y - 0.1f, 0), new Vector3(right.x, pos.y + 0.1f, 0));
                Gizmos.DrawLine(pos + Vector3.left * extX, pos + Vector3.right * extX);
            }
        }

        // Raycasts de GroundWalker
        Gizmos.color = gizmoRayColor;
        if (groundCheck)
        {
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
        if (wallCheck)
        {
            Vector3 dir = (Application.isPlaying ? (facingRight ? Vector3.right : Vector3.left) : Vector3.right);
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + dir * wallCheckDistance);
        }
    }
}
