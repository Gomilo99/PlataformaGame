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
    
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            GameManager.Instance.PerderVida();

            // Aplicamos golpe al personaje
            other.gameObject.GetComponent<CharacterController>().AplicarGolpe();
            AudioManager.Instance.ReproducirSonido(sonidoDano);

            RecibirDano(other.gameObject.GetComponent<CharacterController>().ataque);
            Invoke("ReactivarAtaque", cooldownAtaque);
        }
    }
    private void RecibirDano(float ataqueRecibido)
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