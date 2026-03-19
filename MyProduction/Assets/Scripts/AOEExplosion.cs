using UnityEngine;

public class AOEExplosion : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] private float baseRadius       = 2f;    // radius with 1 AOE stack
    [SerializeField] private float radiusPerStack   = 0.5f;  // added radius per additional stack
    [SerializeField] private float explosionLifetime = 0.2f; // how long before self-destruct

    [Header("Visual Scale")]
    [SerializeField] private bool scaleVisualToRadius = true; // scale sprite to match radius

    private float damage     = 1f;
    private int   aoeStacks  = 1;
    private float finalRadius;

    // Called immediately after instantiation by Projectile or PlayerShooting
    public void Initialise(float dmg, int stacks)
    {
        damage    = dmg;
        aoeStacks = Mathf.Max(1, stacks);
        finalRadius = baseRadius + (radiusPerStack * (aoeStacks - 1));

        if (scaleVisualToRadius)
            transform.localScale = Vector3.one * finalRadius;

        // Apply damage immediately to all enemies in radius
        Explode();

        // Destroy after a short lifetime (long enough for particles to play)
        Destroy(gameObject, explosionLifetime);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, finalRadius);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"[AOE] Hit {hit.name} for {damage} dmg (radius {finalRadius}, stacks {aoeStacks})");
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, baseRadius);
    }
}
