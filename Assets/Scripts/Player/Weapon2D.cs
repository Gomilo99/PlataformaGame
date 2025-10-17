using UnityEngine;

public class Weapon2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CharacterController player;    // para saber facingRight (o pásalo como bool)
    [Header("Tuning")]
    [SerializeField] public float weaponDamage = 1;
    [Tooltip("1 = normal damage, 0 = no damage, 2 = double damage")]
    [SerializeField][Range(0f, 2f)] private float damageMultiplier = 1f;
    [Header("Sounds")]
    [SerializeField] private AudioClip attackSound;
    private float _mulDamage;

    void Start()
    {
        weaponDamage *= damageMultiplier;
        _mulDamage = damageMultiplier;
    }
    void Update()
    {
        if (damageMultiplier != _mulDamage)
        {
            weaponDamage *= damageMultiplier;
            _mulDamage = damageMultiplier;
        }
    }
    public float GetDamage()
    {
        return weaponDamage;
    }
    public void OnEnable()
    {
        AudioManager.Instance.ReproducirSonido(attackSound);
    }
}