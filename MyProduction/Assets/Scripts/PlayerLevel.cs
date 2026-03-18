using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int baseXPRequired = 10;
    [SerializeField] private float xpMultiplierPerLevel = 1.5f;

    [Header("Base Stats (increased by level-up choices)")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private int baseProjectileCount = 1;

    private int xpRequiredForNextLevel;
    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        CalculateXPRequired();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        while (currentXP >= xpRequiredForNextLevel)
            LevelUp();
    }

    private void LevelUp()
    {
        currentXP -= xpRequiredForNextLevel;
        currentLevel++;
        CalculateXPRequired();
        Debug.Log($"LEVEL UP! Now level {currentLevel}");

        if (LevelUpUI.Instance != null)
            LevelUpUI.Instance.ShowLevelUpScreen();
    }

    private void CalculateXPRequired()
    {
        xpRequiredForNextLevel = Mathf.RoundToInt(
            baseXPRequired * Mathf.Pow(xpMultiplierPerLevel, currentLevel - 1));
    }


    public void IncreaseDamage(float amount)
    {
        baseDamage += amount;
        Debug.Log($"Base Damage → {baseDamage}");
        // Damage is read on-demand by PlayerShooting, no push needed.
    }

    public void IncreaseAttackSpeed(float amount)
    {
        baseAttackSpeed += amount;
        Debug.Log($"Base Attack Speed → {baseAttackSpeed}");
        // Attack speed is also read on-demand, no push needed.
    }

    public void IncreaseMoveSpeed(float amount)
    {
        baseMoveSpeed += amount;
        Debug.Log($"Base Move Speed → {baseMoveSpeed}");

        // Push the new speed immediately.
        // If PlayerStats has item bonuses, let it recalculate with the new base.
        // If there are no items yet, push directly to PlayerMovement.
        NotifyStatsChanged();
    }

    public void IncreaseProjectileCount(int amount)
    {
        baseProjectileCount += amount;
        Debug.Log($"Projectile Count → {baseProjectileCount}");
    }

    public void IncreaseMaxHealth(float amount)
    {
        baseMaxHealth += amount;
        PlayerHealth health = GetComponent<PlayerHealth>();
        health?.IncreaseMaxHealth(amount);
        Debug.Log($"Base Max Health → {baseMaxHealth}");
    }

    private void NotifyStatsChanged()
    {
        if (playerStats != null)
        {
            // Trigger a recalc by adding 0 of the first inventory item —
            // cleanest way without exposing a public Recalculate() method
            // is to just call a dedicated refresh.
            playerStats.RefreshStats();
        }
        else
        {
            // No items yet — push speed directly
            PlayerMovement movement = GetComponent<PlayerMovement>();
            movement?.SetMoveSpeed(baseMoveSpeed);
        }
    }

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public float XPProgress => (float)currentXP / xpRequiredForNextLevel;
    public float BaseDamage => baseDamage;
    public float BaseAttackSpeed => baseAttackSpeed;
    public float BaseMoveSpeed => baseMoveSpeed;
    public int BaseProjectileCount => baseProjectileCount;
}