using UnityEngine;

public class PlayerRegen : MonoBehaviour
{
    private float regenPerSecond = 0f;
    private float regenTimer     = 0f;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        // No regen to apply
        if (regenPerSecond <= 0f) return;

        // Don't regen while dead / game over
        if (playerHealth == null) return;

        regenTimer += Time.deltaTime;

        // Tick once per second
        if (regenTimer >= 1f)
        {
            regenTimer -= 1f;

            // Only heal if not already at max
            if (playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                playerHealth.Heal(regenPerSecond);
                Debug.Log($"[PlayerRegen] Healed {regenPerSecond} HP. " +
                          $"Now: {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}");
            }
        }
    }

    /// <summary>Called by PlayerStats whenever items change.</summary>
    public void SetRegenRate(float rate)
    {
        regenPerSecond = rate;
    }

    public float RegenPerSecond => regenPerSecond;
}
