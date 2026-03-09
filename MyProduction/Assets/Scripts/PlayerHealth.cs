using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float invincibilityDuration = 1f;

    [Header("UI References")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private CanvasGroup damageFlash; // Red flash overlay
    [SerializeField] private TextMeshProUGUI healthText;

    private float currentHealth;
    private bool isInvincible = false;
    private float invincibilityTimer = 0f;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            spriteRenderer.enabled = Mathf.Sin(Time.time * 30f) > 0;

            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                spriteRenderer.enabled = true;
            }
        }

        if (damageFlash != null && damageFlash.alpha > 0)
        {
            damageFlash.alpha -= Time.deltaTime * 2f;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateHealthBar();

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        if (damageFlash != null)
        {
            damageFlash.alpha = 0.5f;
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.3f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthBar();
    }

    public void IncreaseMaxHealth(float amount)
    {
        maxHealth += amount;
        currentHealth += amount;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth / maxHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        gameObject.SetActive(false);
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    TakeDamage(10);
    //}

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
}