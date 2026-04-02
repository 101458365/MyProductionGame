using UnityEngine;

public class AOEExplosion : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private float baseRadius = 2f;
    [SerializeField] private float radiusPerStack = 0.5f;
    [SerializeField] private float explosionLifetime = 0.2f;

    [Header("Damage")]
    // AOE deals this percentage of the damage that triggered it
    // 0.3 = 30% of hit damage — strong enough to feel impactful, not a one-shot
    [SerializeField] private float damagePercent = 0.3f;

    [Header("Visual Scale")]
    [SerializeField] private bool scaleVisualToRadius = true;

    private float damage = 1f;
    private int aoeStacks = 1;
    private float finalRadius;

    private void Awake()
    {
        Destroy(gameObject, explosionLifetime + 0.5f);
    }

    public void Initialise(float triggerDamage, int stacks)
    {
        // AOE damage is a percentage of whatever damage triggered it
        damage = triggerDamage * damagePercent;
        aoeStacks = Mathf.Max(1, stacks);
        finalRadius = baseRadius + (radiusPerStack * (aoeStacks - 1));

        if (scaleVisualToRadius)
            transform.localScale = Vector3.one * finalRadius;

        Explode();
        Destroy(gameObject, explosionLifetime);
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
                Debug.Log($"[AOE] Hit {hit.name} for {damage} dmg ({damagePercent * 100f}% of trigger, radius {finalRadius})");
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