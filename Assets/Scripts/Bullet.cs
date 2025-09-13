using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private float deathTime;
    private System.Action<Bullet> _onReturnToPool; // callback al pool

    [Header("Collision")]
    [SerializeField] private string enemyTag = "Enemigo";
    [SerializeField] private string impactTriggerId = "ImpactedBall"; // Trigger del Animator del proyectil
    [SerializeField] private float impactReturnDelay = 0.2f; // Duración de la animación de impacto
    [SerializeField] private AudioClip collisionSound; // Sonido de colisión
    private bool isImpacting = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Recomendado para proyectiles
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Fire(Vector2 position, Vector2 direction, float customSpeed, int customDamage, System.Action<Bullet> onReturnToPool)
    {
        transform.position = new Vector3(position.x, position.y, transform.position.z);
        gameObject.SetActive(true);

        _onReturnToPool = onReturnToPool;
        speed = customSpeed > 0 ? customSpeed : speed;
        damage = customDamage > 0 ? customDamage : damage;

        // normaliza y aplica velocidad
        Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
        rb.velocity = dir * speed;

        AudioManager.Instance.ReproducirSonido(collisionSound);
        deathTime = Time.time + lifetime;
    }

    void Update()
    {
        if (Time.time >= deathTime)
            ReturnToPool();
    }
    private void ReturnToPool()
    {
        rb.velocity = Vector2.zero;
        gameObject.SetActive(false);
        _onReturnToPool?.Invoke(this);
    }

    IEnumerator ReturnAfterImpact(Collider2D col)
    {
        yield return new WaitForSeconds(impactReturnDelay);
        if (col) col.enabled = true;
        isImpacting = false;
        ReturnToPool();
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isImpacting) return;
        if (!other.CompareTag(enemyTag)) return;

        isImpacting = true;
        other.gameObject.GetComponent<Enemigo>()?.RecibirDanoFoe(damage);
        // Detener movimiento y evitar múltiples triggers
        rb.velocity = Vector2.zero;
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // Activar animación de colisión en la bala (requiere Animator con trigger "Impact")
        var anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.ResetTrigger(impactTriggerId);
            anim.SetTrigger(impactTriggerId);
            StartCoroutine(ReturnAfterImpact(col));
        }
        else
        {
            // Si no hay animación, volver al pool inmediatamente
            if (col) col.enabled = true;
            isImpacting = false;
            ReturnToPool();
        }
    }

}