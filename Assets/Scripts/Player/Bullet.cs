using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class Bullet : MonoBehaviour
{
    [Header("Tuning")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 2.5f;
    [SerializeField] private float damage = 1;

    private Rigidbody2D rb;
    private Collider2D col;
    private float deathTime;
    private System.Action<Bullet> _onReturnToPool; // callback al pool

    [Header("Collision")]
    [SerializeField] private string enemyTag = "Enemigo"; // fallback si no se usa layer
    [SerializeField] private LayerMask enemyLayers; // capas que puede dañar
    [SerializeField] private string impactTriggerId = "ImpactedBall"; // Trigger del Animator del proyectil
    [SerializeField] private float impactReturnDelay = 0.2f; // Duración de la animación de impacto
    [SerializeField] private AudioClip collisionSound; // Sonido de colisión
    private bool isImpacting = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (!rb)
        {
            Debug.LogError("Rigidbody2D no encontrado en " + gameObject.name);
            return;
        }
        if(!col)
        {
            Debug.LogError("Collider2D no encontrado en " + gameObject.name);
            return;
        }

        // Recomendado para proyectiles
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Fire(Vector2 position, Vector2 direction, float customSpeed, float customDamage, System.Action<Bullet> onReturnToPool)
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
        bool layerMatch = ((1 << other.gameObject.layer) & enemyLayers) != 0;
        if (!layerMatch && !other.CompareTag(enemyTag)) return;

        isImpacting = true;
        // Detener movimiento y evitar múltiples triggers
        rb.velocity = Vector2.zero;

        col.enabled = false;

        // Activar animación de colisión en la bala (requiere Animator con trigger "Impact")
        var anim = GetComponent<Animator>();
            anim.ResetTrigger(impactTriggerId);
            anim.SetTrigger(impactTriggerId);
            StartCoroutine(ReturnAfterImpact(col));
    }
    public float GetDamage()
    {
        return damage;
    }
}