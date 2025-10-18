using UnityEngine;

/// <summary>
/// Hitbox de arma del enemigo. Debe estar en un GameObject hijo con un Collider2D (isTrigger = true) y opcionalmente un Rigidbody2D Kinematic.
/// Se activa durante la animación de ataque mediante EnemyAttackAnimatorBridge.
/// Al entrar en contacto con el jugador, aplica daño usando la interfaz IPlayerDamageable o buscando un componente conocido.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyWeaponHitbox : MonoBehaviour
{
    [SerializeField] public float Damage = 1f;
    [SerializeField] private LayerMask playerMask; // capas válidas para golpear
    [SerializeField] private AudioClip sonidoAtaque;

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        gameObject.SetActive(false); // se activa sólo durante el ataque
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || !gameObject.activeInHierarchy) return;

        // Filtrar por capa o tag
        if (!((playerMask.value & (1 << other.gameObject.layer)) != 0)) return;
        if (sonidoAtaque && AudioManager.Instance)
            AudioManager.Instance.ReproducirSonido(sonidoAtaque);

    }
}
