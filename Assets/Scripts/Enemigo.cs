using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Parametros")]
    [SerializeField] public float vida = 10;
    [SerializeField] public float cooldownAtaque;
    [SerializeField] public AudioClip sonidoDano;
    [SerializeField] public AudioClip sonidoMuerte;
    private SpriteRenderer spriteRenderer;
    private bool puedeAtacar = true;
    private bool mirandoDerecha = false;
    public Animator animator;
    public new Rigidbody2D rigidbody;
    // Parametros de movimiento

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        if (rigidbody.velocity.x != 0)
        {
            animator.SetBool("isRunningRigth", false);
        }
        else
        {
            animator.SetBool("isRunningRigth", true);
            GestionarMovimiento(rigidbody.velocity.x);
        }
    }
    private void GestionarMovimiento(float inputMovimiento)
    {
        if ((mirandoDerecha && inputMovimiento < 0) || (!mirandoDerecha && inputMovimiento > 0))
        {
            mirandoDerecha = !mirandoDerecha;
            transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);
        }
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Si no puede atacar salimos de la funcion
            if (!puedeAtacar) return;

            // Desactivamos el ataque
            puedeAtacar = false;

            // Cambiamos opacidad del sprite
            Color color = spriteRenderer.color;
            color.a = 0.5f;
            spriteRenderer.color = color;


            // Jugador una vida
            other.gameObject.GetComponent<CharacterController>().PerderVidaPJ();

            // Aplicamos golpe al personaje
            other.gameObject.GetComponent<CharacterController>().AplicarGolpe();
            AudioManager.Instance.ReproducirSonido(sonidoDano);

            RecibirDanoFoe(other.gameObject.GetComponent<CharacterController>().ataque);
            Invoke("ReactivarAtaque", cooldownAtaque);
        }
    }
    public void RecibirDanoFoe(float ataqueRecibido)
    {
        vida -= ataqueRecibido;
        if (vida <= 0)
        {
            Destroy(this.gameObject);
            AudioManager.Instance.ReproducirSonido(sonidoMuerte);
        }

    }
    void ReactivarAtaque()
    {
        puedeAtacar = true;

        // Recuperamos opacidad.
        Color c = spriteRenderer.color;
        c.a = 1f;
        spriteRenderer.color = c;
    }
}