using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{ 
    // Animator params (optimizados con hash)
    private static string isJumpingId = "TriggerJump";
    private static string isAttackedId = "TriggerAttacked";
    private static string isRunningId = "isRunning";
    private static string isAttackingId = "TriggerAttacking";

    [Header("Moving Parameters")]
    [SerializeField] public float velocidad = 5;
    [SerializeField] public bool mirandoDerecha = true;

    [Header("Jumping Parameters")]
    [SerializeField] public float fuerzaSalto = 5;
    [SerializeField] public int saltosMax;
    [Tooltip("Distancia vertical desde la base del collider para comprobar si estamos en suelo (OverlapBox).")]
    [SerializeField] private float groundCheckDistance = 0.08f;
    [Tooltip("Multiplicador del ancho del box usado en la comprobación de suelo (evita falsos negativos en los bordes).")]
    [SerializeField] private float groundCheckWidthMultiplier = 0.9f;
    [Tooltip("Tiempo en segundos tras salir del suelo durante el cual aún se permite saltar (coyote time).")]
    [SerializeField] private float coyoteTime = 0.12f;
    [Tooltip("Tiempo en segundos durante el cual un pulso de salto previo al aterrizaje se recuerda (jump buffer).")]
    [SerializeField] private float jumpBufferTime = 0.12f;

    [Header("Hit Parameters")]
    [SerializeField] public float fuerzaGolpe;
    [SerializeField] public float ataque = 1;
    [SerializeField] public LayerMask enemyWeaponMask;
    [SerializeField] public AudioClip sonidoAtacado;

    [Header("Object References")]
    [SerializeField] public Muzzle2D muzzle;
    [SerializeField] public LayerMask capaSuelo;
    [SerializeField] public LayerMask deathMask;
    [SerializeField] public AudioClip audioSalto;
    [Header("Player Components")]
    [SerializeField] public PlayerStats playerStats;

    private Animator animator;
    private int saltosRestantes;
    private new Rigidbody2D rigidbody;
    private BoxCollider2D boxCollider;
    // Estado para coyote time / jump buffer
    private float _lastGroundedTime = -10f;
    private float _lastJumpPressedTime = -10f;
    private bool _wasGrounded = false;
    
    private bool puedeMoverse = true;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        saltosRestantes = saltosMax;
        animator = GetComponent<Animator>();
        if (!playerStats) playerStats = GetComponent<PlayerStats>();

    }
    // Update is called once per frame
    void Update()
    {
        // Inputs centralizados aquí
        // Pausa
        if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance)
        {
            GameManager.Instance.TogglePause();
        }

        // Movimiento / salto / ataque
        ProcesarSalto();
        ProcesarMovimiento();
        ProcesarAtaque();
    }
    void ProcesarAtaque()
    {
        // Disparar arma
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (muzzle != null) muzzle.TryFire();
        }
        // Golpe con espada (animacion)
        if (Input.GetKeyUp(KeyCode.F))
        {
            animator.SetTrigger(isAttackingId);
        }
    }

    void ProcesarSalto()
    {
        // Registrar pulsación de salto (para jump buffer)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _lastJumpPressedTime = Time.time;
        }

        bool grounded = EstaEnSuelo();
        if (grounded)
        {
            _lastGroundedTime = Time.time;
            // Si acabamos de aterrizar (transición), restaurar saltos
            if (!_wasGrounded)
            {
                saltosRestantes = saltosMax;
            }
        }

        // Ejecutar salto si se cumple cualquiera de las condiciones:
        // - Hay saltos disponibles (double jump)
        // - Estamos dentro del coyote time
        bool canUseCoyote = (Time.time - _lastGroundedTime) <= coyoteTime;
        bool jumpBuffered = (Time.time - _lastJumpPressedTime) <= jumpBufferTime;

        if (jumpBuffered && (saltosRestantes > 0 || canUseCoyote))
        {
            // Consumir buffer
            _lastJumpPressedTime = -10f;

            // Realizar salto
            saltosRestantes = Mathf.Max(0, saltosRestantes - 1);
            animator.SetTrigger(isJumpingId);
            // Reiniciar velocidad vertical para saltos consistentes
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
            rigidbody.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            if (AudioManager.Instance) AudioManager.Instance.ReproducirSonido(audioSalto);
        }

        _wasGrounded = grounded;
    }

    bool EstaEnSuelo()
    {
        // Usar OverlapBox justo por debajo del collider para evitar interferencias con el propio collider
        Vector2 boxCenter = boxCollider.bounds.center;
        float halfHeight = boxCollider.bounds.extents.y;
        float halfWidth = boxCollider.bounds.extents.x * groundCheckWidthMultiplier;

        Vector2 checkCenter = boxCenter + Vector2.down * (halfHeight + groundCheckDistance);
        Vector2 checkSize = new Vector2(halfWidth * 2f, groundCheckDistance * 2f);

        Collider2D hit = Physics2D.OverlapBox(checkCenter, checkSize, 0f, capaSuelo);
        return hit != null;
    }
    void ProcesarMovimiento()
    {
        if (!puedeMoverse) return;

        float inputMovimiento = Input.GetAxisRaw("Horizontal");
        if (inputMovimiento != 0f)
        {
            animator.SetBool(isRunningId, true);
        }
        else
        {
            animator.SetBool(isRunningId, false);
        }
        rigidbody.velocity = new Vector2(inputMovimiento * velocidad, rigidbody.velocity.y);
        GestionarMovimiento(inputMovimiento);
    }
    void GestionarMovimiento(float inputMovimiento)
    {
        if ((mirandoDerecha && inputMovimiento < 0) || (!mirandoDerecha && inputMovimiento > 0))
        {
            mirandoDerecha = !mirandoDerecha;
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
    }
    public void AplicarGolpe()
    {
        // Desactivar movimiento mientras dura la reacción al golpe
        puedeMoverse = false;
        animator.SetTrigger(isAttackedId);

        // Determinar dirección del empuje basado en la orientación del personaje
        int direccionX = mirandoDerecha ? -1 : 1; // empujar hacia atrás del atacante (si mira a la derecha, empujamos a la izquierda)

        // Construir vector de fuerza y normalizar para mantener proporciones controladas
        Vector2 direccionGolpe = new Vector2(direccionX, 1).normalized;

        // Reiniciar la velocidad vertical para que el impulso sea consistente
        rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);

        // Aplicar la fuerza como impulso para obtener un empujón instantáneo
        rigidbody.AddForce(direccionGolpe * fuerzaGolpe, ForceMode2D.Impulse);

        Debug.Log("AplicarGolpe: dirección=" + direccionGolpe + " fuerza=" + fuerzaGolpe);

        StartCoroutine(EsperarYActivarMovimiento());
    }
    public void PerderVidaPJ()
    {
        animator.SetTrigger(isAttackedId);
        // Aseguramos que no quede atrapado si el estado necesita salir.
        StartCoroutine(ResetAttackedFlagSafeguard());
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Zona de muerte -> dispara evento de muerte del jugador y abre panel de muerte
        if ((deathMask.value & (1 << collision.gameObject.layer)) != 0)
        {
            Debug.Log("Has muerto! Zona de muerte detectada.");
            if (GameManager.Instance)
            {
                GameManager.Instance.TriggerPlayerDeath();
            }
            else
            {
                // Fallback si no hay GameManager
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Zona de muerte -> dispara evento de muerte del jugador y abre panel de muerte
        
        // Filtrar por capa o tag
        if (!((enemyWeaponMask.value & (1 << other.gameObject.layer)) != 0)) return;

        var enemyHitBox = other.gameObject.GetComponent<EnemyWeaponHitbox>();
        if (!enemyHitBox)
        {
            Debug.LogWarning("El objeto que colisiona no tiene componente EnemyWeaponHitbox: " + other.gameObject.name);
            return;
        }

    if (playerStats) playerStats.TakeDamage(Mathf.RoundToInt(enemyHitBox.Damage));
        AplicarGolpe();
        if (sonidoAtacado && AudioManager.Instance)
            AudioManager.Instance.ReproducirSonido(sonidoAtacado);

    }
    IEnumerator EsperarYActivarMovimiento()
    {
        // Wait before checking if grounded.
        yield return new WaitForSeconds(0.1f);
        while (!EstaEnSuelo())
        {
            // Esperamos al siguiente frame
            yield return null;
        }
        // Si ya está en el suelo activamos el movimiento.
        puedeMoverse = true;
    }
    private IEnumerator ResetAttackedFlagSafeguard()
    {
        // Espera un pequeño tiempo para permitir reproducir la animación y luego limpia si hay bool asociado.
        yield return new WaitForSeconds(0.25f);
        // Si tu animator utiliza un bool en lugar de puro trigger, puedes desactivarlo aquí:
        // animator.SetBool("isAttacked", false);
        // Forzamos a permitir re-disparar el trigger
        animator.ResetTrigger(isAttackedId);
    }
}
