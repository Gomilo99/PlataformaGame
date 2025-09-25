using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemigo : MonoBehaviour
{
    [Header("Parametros")]
    [SerializeField] public float vida = 10;
    [SerializeField] public float cooldownAtaque;
    [SerializeField] public AudioClip sonidoDano;
    [SerializeField] public AudioClip sonidoMuerte;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyPatrol2D patrol;

    [Header("Animator / Estados")]
    [SerializeField] private Animator animator; // si se asigna, se actualizará un bool de correr
    [SerializeField] static public string deathTrigger = "deadTrigger";
    [SerializeField] static public string attackedTrigger = "attackedTrigger";
    [SerializeField] static public string attackingTrigger = "attackingTrigger";
    [SerializeField] static public string isRunning = "isRunning";
    [SerializeField] private LayerMask playerWeaponMask;

    [Header("Multiplicador de daño por capa")]
    [SerializeField] private bool enableDamageMultiplierOnWeaponMask = false;
    [SerializeField][Range(0f, 2f)] private float damageMultiplier = 1f; // 1 = daño normal, 0 = inmune, 2 = doble daño
    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        patrol = GetComponent<EnemyPatrol2D>();
        if (!animator)
        {
            Debug.LogError("Animator no encontrado en " + gameObject.name);
            return;
        }

        if (!rb)
        {
            Debug.LogError("Rigidbody2D no encontrado en " + gameObject.name);
            return;
        }
        if (!patrol)
        {
            Debug.LogError("EnemyPatrol2D no encontrado en " + gameObject.name);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verifica que el objeto pertenezca a alguna capa seleccionada en playerWeaponMask
        if (!((playerWeaponMask.value & (1 << other.gameObject.layer)) != 0)) return;


        Debug.Log("Golpeado por arma del jugador");
        var weapon = other.gameObject.GetComponent<Bullet>();
        if (!weapon)
        {
            Debug.LogWarning("El objeto que colisiona no tiene componente Bullet: " + other.gameObject.name);
            return;
        }
        // Calcula daño (con reducción opcional si está habilitada)
        float incomingDamage = weapon.GetDamage();
        if (enableDamageMultiplierOnWeaponMask)
        {
            // damageMultiplier en [0..2]: 1 = 100% del daño, 0.5 = 50%, 2 = 200%, 0 = inmune
            incomingDamage *= Mathf.Clamp(damageMultiplier, 0f, 2f);
        }

        vida -= incomingDamage;
        if (vida <= 0)
        {
            animator.SetTrigger(deathTrigger);
            rb.velocity = Vector2.zero;
            AudioManager.Instance.ReproducirSonido(sonidoMuerte);
            StartCoroutine(WaitForDeathAnimation());
        }
        else
        {
            animator.SetTrigger(attackedTrigger);
            rb.velocity = Vector2.zero;
            AudioManager.Instance.ReproducirSonido(sonidoDano);
            StartCoroutine(ResetAttackedFlagSafeguard());
        }
    }

    // Funciones Iteradoras
    private IEnumerator ResetAttackedFlagSafeguard()
    {
        // Espera un pequeño tiempo para permitir reproducir la animación y luego limpia si hay bool asociado.
        float attackedAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(attackedAnimationTime);
        animator.ResetTrigger(attackedTrigger);
    }
    private IEnumerator WaitForDeathAnimation()
    {
        // Espera a que termine la animación de muerte antes de destruir el objeto.
        float deadAnimationTime = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deadAnimationTime);
        Destroy(gameObject);
    }

}