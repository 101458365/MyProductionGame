using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int baseXPRequired = 10;
    [SerializeField] private float xpMultiplierPerLevel = 1.5f;

    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private int baseProjectileCount = 1;

    private int xpRequiredForNextLevel;
    private PlayerStats playerStats;
    private DismantleAbility dismantleAbility;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        dismantleAbility = GetComponent<DismantleAbility>();
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
    }

    public void IncreaseAttackSpeed(float amount)
    {
        baseAttackSpeed += amount;
        Debug.Log($"Base Attack Speed → {baseAttackSpeed}");
    }

    public void IncreaseMoveSpeed(float amount)
    {
        baseMoveSpeed += amount;
        Debug.Log($"Base Move Speed → {baseMoveSpeed}");
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
        GetComponent<PlayerHealth>()?.IncreaseMaxHealth(amount);
        Debug.Log($"Base Max Health → {baseMaxHealth}");
    }

    // Adds one Dismantle slash stack — unlocks the ability on first pick
    public void IncreaseDismantle(int amount)
    {
        if (dismantleAbility == null)
        {
            Debug.LogWarning("[PlayerLevel] DismantleAbility component not found on Player!");
            return;
        }

        for (int i = 0; i < amount; i++)
            dismantleAbility.AddStack();

        Debug.Log($"Dismantle unlocked — slash count mirrors projectile count.");
    }

    private void NotifyStatsChanged()
    {
        if (playerStats != null)
            playerStats.RefreshStats();
        else
            GetComponent<PlayerMovement>()?.SetMoveSpeed(baseMoveSpeed);
    }

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public float XPProgress => (float)currentXP / xpRequiredForNextLevel;
    public float BaseDamage => baseDamage;
    public float BaseAttackSpeed => baseAttackSpeed;
    public float BaseMoveSpeed => baseMoveSpeed;
    public int BaseProjectileCount => baseProjectileCount;
}