using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float damageCooldown = 0.5f;

    [Header("Gold Drop")]
    [SerializeField] private int baseGoldDrop = 3;
    [SerializeField] private int goldDropVariance = 2; // drops baseGold ± variance

    private float currentHealth;
    private float lastDamageTime = 0f;

    // Set by EnemySpawner when this enemy is spawned as an elite
    private bool isElite = false;
    private float eliteGoldMultiplier = 3f;

    private void Start()
    {
        if (DifficultyManager.Instance != null)
        {
            maxHealth *= DifficultyManager.Instance.HPMultiplier;
            contactDamage *= DifficultyManager.Instance.DamageMultiplier;

            // Elites scale even harder with difficulty
            if (isElite)
            {
                maxHealth *= 2.5f;
                contactDamage *= 1.5f;
            }
        }

        currentHealth = maxHealth;
    }

    public void SetElite(bool elite, float goldMultiplier = 3f)
    {
        isElite = elite;
        eliteGoldMultiplier = goldMultiplier;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage! HP: {currentHealth}/{maxHealth}");
        StartCoroutine(FlashWhite());

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.AddKill();

        if (DropManager.Instance != null)
        {
            if (isElite)
                DropManager.Instance.SpawnEliteDrops(transform.position);
            else
                DropManager.Instance.SpawnDrops(transform.position);
        }

        // Drop gold (elites drop more)
        if (GoldManager.Instance != null)
        {
            int gold = baseGoldDrop + Random.Range(-goldDropVariance, goldDropVariance + 1);
            gold = Mathf.Max(1, gold); // always at least 1
            if (isElite) gold = Mathf.RoundToInt(gold * eliteGoldMultiplier);
            DropManager.Instance?.SpawnGold(transform.position, gold);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

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

    private System.Collections.IEnumerator FlashWhite()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color original = sr.color;

        // Elites flash gold instead of white
        sr.color = isElite ? new Color(1f, 0.85f, 0f) : Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }

    public void SetMaxHealth(float health) { maxHealth = health; currentHealth = health; }
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public bool IsElite => isElite;
}
