using UnityEngine;

/// <summary>
/// Comportamiento de persecución simple: si el jugador entra en radio de detección,
/// se suspende la patrulla y el enemigo se mueve hacia el jugador. Si sale del radio + hysteresis, vuelve a patrullar.
/// Depende de un Rigidbody2D y opcionalmente de EnemyPatrol2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaseBehaviour : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float loseRadiusExtra = 1.5f; // margen para no cortar persecución inmediatamente
    [SerializeField] private LayerMask playerMask; // capa del jugador
    [SerializeField] private Transform player; // asignar si se quiere cachear, si no se buscará por tag

    [Header("Movimiento persecución")]
    [SerializeField] private float chaseSpeedMultiplier = 1.2f; // multiplica speed base del patrullero
    [SerializeField] private float verticalTolerance = 1.5f; // si la diferencia vertical supera esto, puede que no persiga

    [Header("Ataque")]
    [SerializeField] private float attackRange = 0.7f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private static readonly int attackTrigger = Animator.StringToHash("TriggerAttacked"); // Animator trigger (puede mapearse a isAttacking)
    [SerializeField] private AudioClip attackSound;

    [Header("Refs")]
    [SerializeField] private EnemyPatrol2D patrol; // puede ser null si no se patrulla
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private float baseSpeed; // speed original del patrullero
    private bool chasing;
    private float nextAttackTime;
    private float loseRadius; // detectionRadius + loseRadiusExtra

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        if (!patrol) patrol = GetComponent<EnemyPatrol2D>();
        if (patrol)
        {
            var speedField = typeof(EnemyPatrol2D).GetField("speed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (speedField != null)
                baseSpeed = (float)speedField.GetValue(patrol);
        }
        loseRadius = detectionRadius + loseRadiusExtra;
    }

    void Start()
    {
        if (!player)
        {
            var plyGO = GameManager.Instance.player;
            //var plyGO = GameObject.FindGameObjectWithTag("Player");
            if (plyGO) player = plyGO.transform;
        }
    }

    void Update()
    {
        if (!player) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (!chasing)
        {
            // Entrar a persecución
            if (dist <= detectionRadius && Mathf.Abs(player.position.y - transform.position.y) <= verticalTolerance)
            {
                chasing = true;
                if (patrol) patrol.Pause();
            }
        }
        else
        {
            // Salir de persecución si excede loseRadius
            if (dist > loseRadius)
            {
                chasing = false;
                if (patrol) patrol.Resume();
            }
        }

        if (chasing)
        {
            Perseguir();
            IntentarAtacar(dist);
        }
    }

    private void Perseguir()
    {
        if (!player) return;
        // movimiento horizontal hacia el jugador
        Vector2 dir = player.position - transform.position;
        dir.y = 0f; // opcional: mantener solo eje X
        dir.Normalize();

        float effectiveSpeed = baseSpeed > 0 ? baseSpeed * chaseSpeedMultiplier : 2f * chaseSpeedMultiplier;
        rb.velocity = new Vector2(dir.x * effectiveSpeed, rb.velocity.y);

        // Flip manual si no patrulla
        if (!patrol)
        {
            if (Mathf.Abs(dir.x) > 0.01f)
            {
                Vector3 s = transform.localScale;
                s.x = dir.x > 0 ? Mathf.Abs(s.x) : -Mathf.Abs(s.x);
                transform.localScale = s;
            }
        }
    }

    private void IntentarAtacar(float currentDistance)
    {
        if (currentDistance > attackRange) return;
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        if (animator)
        {
            animator.ResetTrigger(attackTrigger);
            animator.SetTrigger(attackTrigger);
        }
        if (attackSound && AudioManager.Instance)
        {
            AudioManager.Instance.ReproducirSonido(attackSound);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius + loseRadiusExtra);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
