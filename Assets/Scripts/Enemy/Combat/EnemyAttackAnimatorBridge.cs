using UnityEngine;

/// <summary>
/// Puente para Animation Events: habilita/deshabilita hitboxes (collider/GO) del arma del enemigo.
/// Añadir este componente al enemigo y referenciar los GameObjects de hitbox.
/// En la animación de ataque, crear eventos: Enemy_AttackHitbox_On() y Enemy_AttackHitbox_Off().
/// </summary>
public class EnemyAttackAnimatorBridge : MonoBehaviour
{
    [SerializeField] private GameObject[] hitboxObjects; // hijos con EnemyWeaponHitbox

    public void Enemy_AttackHitbox_On()
    {
        SetHitboxesActive(true);
    }

    public void Enemy_AttackHitbox_Off()
    {
        SetHitboxesActive(false);
    }

    private void SetHitboxesActive(bool active)
    {
        if (hitboxObjects == null) return;
        foreach (var go in hitboxObjects)
        {
            if (!go) continue;
            go.SetActive(active);
        }
    }
}
