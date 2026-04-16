using UnityEngine;

public class AOEExplosion : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private float baseRadius = 2f;
    [SerializeField] private float radiusPerStack = 0.5f;

    [Header("Damage")]
    [SerializeField] private float damagePercent = 0.3f;

    [Header("Visual Scale")]
    // The sprite animation will scale from this fraction of finalRadius up to finalRadius
    [SerializeField] private float startScaleFraction = 0.15f;

    private float damage = 1f;
    private int aoeStacks = 1;
    private float finalRadius;

    private void Awake()
    {
        Destroy(gameObject, 2f);
    }

    public void Initialise(float triggerDamage, int stacks)
    {
        damage = triggerDamage * damagePercent;
        aoeStacks = Mathf.Max(1, stacks);
        finalRadius = baseRadius + (radiusPerStack * (aoeStacks - 1));

        // Tell the animation how big to grow based on actual radius
        SpriteSwapAnimation anim = GetComponent<SpriteSwapAnimation>();
        if (anim != null)
            anim.SetScale(finalRadius * startScaleFraction, finalRadius);

        Explode();
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, finalRadius);

        int hitCount = 0;
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                hitCount++;
                Debug.Log($"[AOE] Hit {hit.name} for {damage} dmg (radius {finalRadius})");
            }
        }

        Debug.Log($"[AOE] Explosion hit {hitCount} enemies, radius {finalRadius}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, baseRadius);
    }
}