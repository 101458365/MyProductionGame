using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private float damage    = 1f;
    private bool  damageSet = false;

    // Set by PlayerShooting after instantiation
    private GameObject   aoeExplosionPrefab;
    private int          aoeStacks = 0;
    private float        aoeDamage = 0f;

    private void Awake()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDamage(float newDamage)
    {
        damage    = newDamage;
        damageSet = true;
        Debug.Log($"Projectile damage SET to: {damage}");
    }

    public void SetAOE(GameObject explosionPrefab, int stacks, float explosionDamage)
    {
        aoeExplosionPrefab = explosionPrefab;
        aoeStacks          = stacks;
        aoeDamage          = explosionDamage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Debug.Log($"Projectile hitting enemy with {damage} damage (damage was set: {damageSet})");

            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);

            // Trigger AOE explosion at impact point
            TrySpawnAOE();

            Destroy(gameObject);
        }

        if (collision.CompareTag("Wall"))
            Destroy(gameObject);
    }

    private void TrySpawnAOE()
    {
        if (aoeExplosionPrefab == null || aoeStacks <= 0) return;

        GameObject explosion = Instantiate(aoeExplosionPrefab, transform.position, Quaternion.identity);
        AOEExplosion aoe     = explosion.GetComponent<AOEExplosion>();
        aoe?.Initialise(aoeDamage, aoeStacks);
    }
}
