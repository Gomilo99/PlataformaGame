using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{ 
    // Animator params (optimizados con hash)
    private static readonly int isJumpingId = Animator.StringToHash("IsJumping");
    private static readonly int isAttackedId = Animator.StringToHash("isAttacked");
    private static readonly int isRunningId = Animator.StringToHash("isRunning");
    private static readonly int isAttackingId    = Animator.StringToHash("isAttacking");

    [Header("Moving Parameters")]
    [SerializeField] public float velocidad = 5;
    [SerializeField] public bool mirandoDerecha = true;

    [Header("Jumping Parameters")]
    [SerializeField] public float fuerzaSalto = 5;
    [SerializeField] public int saltosMax;

    [Header("Hit Parameters")]
    [SerializeField] public float fuerzaGolpe;
    [SerializeField] public float ataque = 1;

    [Header("Object References")]
    [SerializeField] public Weapon2D muzzle;
    [SerializeField] public LayerMask capaSuelo;
    [SerializeField] public AudioClip audioSalto;

    private Animator animator;
    private int saltosRestantes;
    private new Rigidbody2D rigidbody;
    private BoxCollider2D boxCollider;
    
    private bool puedeMoverse = true;

    [SerializeField] private int vidas;

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
        puedeMoverse = false;
        Vector2 direccionGolpe;
        int direccionX;
        if (rigidbody.velocity.x > 0)
        {
            direccionX = -1;
        }
        else
        {
            direccionX = 1;
        }

        direccionGolpe = new Vector2(direccionX, 1);
        rigidbody.AddForce(direccionGolpe * fuerzaGolpe);
        StartCoroutine(EsperarYActivarMovimiento());
    }
    public void PerderVidaPJ()
    {
        vidas -= 1;
        if (vidas == 0)
        {
            //Reiniciar Nivel
            SceneManager.LoadScene(0);
        }
        EventBus<int>.Publish(GameEvent.VidaPerdida, vidas);
        animator.SetTrigger(isAttackedId);
        //StartCoroutine(EsperarAnimacionAtacado());
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
    IEnumerator EsperarAnimacionAtacado() {
        
        yield return new WaitForSeconds(0.1f);
        
        animator.SetBool("isAttacked", false);
    }
}
