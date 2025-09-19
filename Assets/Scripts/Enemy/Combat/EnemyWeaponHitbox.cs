using UnityEngine;

/// <summary>
/// Hitbox de arma del enemigo. Debe estar en un GameObject hijo con un Collider2D (isTrigger = true) y opcionalmente un Rigidbody2D Kinematic.
/// Se activa durante la animación de ataque mediante EnemyAttackAnimatorBridge.
/// Al entrar en contacto con el jugador, aplica daño usando la interfaz IPlayerDamageable o buscando un componente conocido.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyWeaponHitbox : MonoBehaviour
{
    [SerializeField] private float damage = 1;
    [SerializeField] private LayerMask playerMask; // capa a la que pertenece el Player
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string attackingTrigger = "TriggerAttacked"; // Animator trigger (puede mapearse a isAttacking)
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
        if (((1 << other.gameObject.layer) & playerMask) == 0 && !other.CompareTag(playerTag))
            return;

        // Intentar aplicar daño al jugador
        //gameObject.GetComponentInParent<Animator>()?.SetTrigger(attackingTrigger);
        other.GetComponent<GameManager>()?.PerderVida(damage);
        AudioManager.Instance.ReproducirSonido(sonidoAtaque);
    }
}
