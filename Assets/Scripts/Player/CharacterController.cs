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

    [Header("Hit Parameters")]
    [SerializeField] public float fuerzaGolpe;
    [SerializeField] public float ataque = 1;
    [SerializeField] public LayerMask enemyWeaponMask;
    [SerializeField] public AudioClip sonidoAtacado;

    [Header("Object References")]
    [SerializeField] public Weapon2D muzzle;
    [SerializeField] public LayerMask capaSuelo;
    [SerializeField] public AudioClip audioSalto;

    private Animator animator;
    private int saltosRestantes;
    private new Rigidbody2D rigidbody;
    private BoxCollider2D boxCollider;
    
    private bool puedeMoverse = true;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        saltosRestantes = saltosMax;
        animator = GetComponent<Animator>();

    }
    // Update is called once per frame
    void Update()
    {
        ProcesarMovimiento();
        ProcesarSalto();
        ProcesarAtaque();
    }
    void ProcesarAtaque()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger(isAttackingId);
            muzzle.TryFire();
        }
    }
    bool EstaEnSuelo()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, new Vector2(boxCollider.bounds.size.x, boxCollider.bounds.size.y), 0f, Vector2.down, 0.2f, capaSuelo);
        return raycastHit.collider != null;
    }
    void ProcesarSalto()
    {
        if (EstaEnSuelo()) saltosRestantes = saltosMax;
        if (Input.GetKeyDown(KeyCode.Space) && saltosRestantes > 0)
        {
            saltosRestantes--;
            animator.SetTrigger(isJumpingId);
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
            rigidbody.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            AudioManager.Instance.ReproducirSonido(audioSalto);
        }
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Filtrar por capa o tag
        if (!((enemyWeaponMask.value & (1 << other.gameObject.layer)) != 0)) return;

        var enemyHitBox = other.gameObject.GetComponent<EnemyWeaponHitbox>();
        if (!enemyHitBox)
        {
            Debug.LogWarning("El objeto que colisiona no tiene componente EnemyWeaponHitbox: " + other.gameObject.name);
            return;
        }

        GameManager.Instance.PerderVida(enemyHitBox.Damage);
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
