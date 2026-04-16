using UnityEngine;

public class PlayerRegen : MonoBehaviour
{
    // regenPerSecond is now treated as a % of max HP per second
    // e.g. 0.02 = 2% of max HP per second per stack
    private float regenPercent = 0f;
    private float regenTimer = 0f;

    private PlayerHealth playerHealth;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (regenPercent <= 0f) return;
        if (playerHealth == null) return;

        regenTimer += Time.deltaTime;

        if (regenTimer >= 1f)
        {
            regenTimer -= 1f;

            if (playerHealth.CurrentHealth < playerHealth.MaxHealth)
            {
                float healAmount = playerHealth.MaxHealth * regenPercent;
                playerHealth.Heal(healAmount);
                Debug.Log($"[PlayerRegen] Healed {healAmount:F1} HP " +
                          $"({regenPercent * 100f:F1}% of {playerHealth.MaxHealth} max HP)");
            }
        }
    }

    // Called by PlayerStats — passes total regen percent across all stacks
    // e.g. 3 stacks at 0.02 each = 0.06 = 6% max HP per second
    public void SetRegenRate(float percent)
    {
        regenPercent = percent;
    }

    public float RegenPercent => regenPercent;
}