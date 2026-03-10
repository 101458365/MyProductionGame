using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP = 0;
    [SerializeField] private int baseXPRequired = 10;
    [SerializeField] private float xpMultiplierPerLevel = 1.5f;

    [Header("Base Stats (Increased on Level Up)")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseMaxHealth = 100f;

    private int xpRequiredForNextLevel;

    private void Start()
    {
        CalculateXPRequired();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;

        // Check for level up
        while (currentXP >= xpRequiredForNextLevel)
        {
            LevelUp();
        }

        // TODO: Update XP bar UI (Commit 7)
    }

    private void LevelUp()
    {
        currentXP -= xpRequiredForNextLevel;
        currentLevel++;
        CalculateXPRequired();

        Debug.Log($"LEVEL UP! Now level {currentLevel}");

        // TODO: Show level up choice screen

    }

    private void CalculateXPRequired()
    {
        xpRequiredForNextLevel = Mathf.RoundToInt(baseXPRequired * Mathf.Pow(xpMultiplierPerLevel, currentLevel - 1));
    }

    // Apply stat increases from level up choices
    public void IncreaseDamage(float amount)
    {
        baseDamage += amount;
        Debug.Log($"Base Damage increased to {baseDamage}!");
    }

    public void IncreaseAttackSpeed(float amount)
    {
        baseAttackSpeed += amount;
        Debug.Log($"Base Attack Speed increased to {baseAttackSpeed}!");
    }

    public void IncreaseMoveSpeed(float amount)
    {
        baseMoveSpeed += amount;

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMoveSpeed(baseMoveSpeed);
        }

        Debug.Log($"Base Move Speed increased to {baseMoveSpeed}!");
    }

    public void IncreaseMaxHealth(float amount)
    {
        baseMaxHealth += amount;

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.IncreaseMaxHealth(amount);
        }

        Debug.Log($"Base Max Health increased to {baseMaxHealth}!");
    }

    // Getters
    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPRequiredForNextLevel => xpRequiredForNextLevel;
    public float XPProgress => (float)currentXP / xpRequiredForNextLevel;
    public float BaseDamage => baseDamage;
    public float BaseAttackSpeed => baseAttackSpeed;
}