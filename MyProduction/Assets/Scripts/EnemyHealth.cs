using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 0.5f;

    private float currentHealth;
    private float lastDamageTime = 0f;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Flash white when hit
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Add kill count
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddKill();
        }

        // Spawn drops
        if (DropManager.Instance != null)
        {
            DropManager.Instance.SpawnDrops(transform.position);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Damage player on contact with cooldown
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(contactDamage);
                    lastDamageTime = Time.time;
                }
            }
        }
    }

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
        currentHealth = health;
    }

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
}