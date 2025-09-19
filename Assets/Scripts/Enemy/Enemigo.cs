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
    [SerializeField] public float damage = 1f;
    private bool puedeAtacar = true;
    private string PlayerWeaponTag = "PlayerWeapon";
    [Header("Animator")]
    [SerializeField] private Animator animator; // si se asigna, se actualizará un bool de correr
    private static readonly int isAttackedId = Animator.StringToHash("isAttacked");
    void Awake()
    {
        // Intentamos autoasignar componentes si no se han asignado en el inspector.
        if (!animator) animator = GetComponent<Animator>();
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(PlayerWeaponTag))
        {
            animator.ResetTrigger(isAttackedId);
            animator.SetTrigger(isAttackedId);

            // Jugador pierde una vida
            //GameManager.Instance.PerderVida();
            //AudioManager.Instance.ReproducirSonido(sonidoDano);

            RecibirDanoFoe(other.gameObject.GetComponent<Weapon2D>().GetBulletDamage());
            StartCoroutine(ResetAttackedFlagSafeguard());
            
        }
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
    public void RecibirDanoFoe(float ataqueRecibido)
    {
        vida -= ataqueRecibido;
        if (vida <= 0)
        {
            Destroy(this.gameObject);
            AudioManager.Instance.ReproducirSonido(sonidoMuerte);
        }
    }
    // Llamado desde evento de animación (frame de impacto) o manualmente
    public void AplicarDanoJugadorEnAtaque()
    {
        if (!puedeAtacar) return;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player) return;
        var pj = player.GetComponent<CharacterController>();
        if (!pj) return;
        pj.PerderVidaPJ();
        pj.AplicarGolpe();
    }
}