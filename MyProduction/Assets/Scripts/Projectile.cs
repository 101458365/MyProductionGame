using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy enemy on hit
        if (collision.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(1f); // 1 damage per projectile
            }
            Destroy(gameObject);
        }

        // Destroy on wall hit
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}