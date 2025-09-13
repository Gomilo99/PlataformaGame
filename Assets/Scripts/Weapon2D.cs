using UnityEngine;

public class Weapon2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BulletPool pool;
    [SerializeField] private CharacterController player;    // para saber facingRight (o pásalo como bool)
    [SerializeField] private Transform muzzle; // punto de salida de la bala (hijo del Player)

    [Header("Tuning")]
    [SerializeField] private float bulletSpeed = 14f;
    [SerializeField] private int bulletDamage = 1;
    [SerializeField] private float fireCooldown = 0.2f;

    private float nextFireTime;
    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;

    void Start()
    {
        if (muzzle == null) muzzle = transform;
        if (player == null || muzzle == null) return;

        // Asegura que el muzzle sea hijo del Player y conserve su posición mundial actual
        muzzle.SetParent(player.transform, true);

        // Guarda la posición/rotación local para mantenerla fija relativa al Player
        initialLocalPos = muzzle.localPosition;
        initialLocalRot = muzzle.localRotation;
    }

    void LateUpdate()
    {
        // Fuerza la posición/rotación local constantes respecto al Player
        muzzle.localPosition = initialLocalPos;
        muzzle.localRotation = initialLocalRot;
    }
    void Awake()
    {
        muzzle = GetComponent<Transform>();
    }
    public void TryFire()
    {
        if (Time.time < nextFireTime) return;

        var bullet = pool.Get();
        if (bullet == null) return;

        // dirección según tu facingRight; o calcula con mouse/aim si quieres
        Vector2 dir = player != null && player.mirandoDerecha ? Vector2.right : Vector2.left;

        bullet.Fire(
            position: muzzle.position,
            direction: dir,
            customSpeed: bulletSpeed,
            customDamage: bulletDamage,
            onReturnToPool: pool.Return
        );

        nextFireTime = Time.time + fireCooldown;
    }
}