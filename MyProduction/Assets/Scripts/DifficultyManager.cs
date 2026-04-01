using UnityEngine;
using TMPro;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance;

    [Header("Difficulty Settings")]
    [SerializeField] private float difficultyIncreaseInterval = 30f; // Every 30 seconds
    [SerializeField] private float hpMultiplierPerTier = 0.2f; // 20% more HP per tier
    [SerializeField] private float damageMultiplierPerTier = 0.15f; // 15% more damage per tier
    [SerializeField] private float speedMultiplierPerTier = 0.1f; // 10% more speed per tier
    [SerializeField] private float spawnRateMultiplierPerTier = 0.15f; // 15% faster spawns per tier

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    private int currentDifficultyTier = 0;
    private float gameTime = 0f;
    private float nextDifficultyIncreaseTime = 30f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Update()
    {
        gameTime += Time.deltaTime;

        // Check for difficulty increase
        if (gameTime >= nextDifficultyIncreaseTime)
        {
            IncreaseDifficulty();
            nextDifficultyIncreaseTime += difficultyIncreaseInterval;
        }

        UpdateUI();
    }

    private void IncreaseDifficulty()
    {
        currentDifficultyTier++;
        Debug.Log($"Difficulty increased to Tier {currentDifficultyTier}!");

        // Camera shake on difficulty increase
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.3f, 0.5f);
        }
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {currentDifficultyTier}";
        }
    }

    // Getters for other systems
    public float HPMultiplier => 1f + (hpMultiplierPerTier * currentDifficultyTier);
    public float DamageMultiplier => 1f + (damageMultiplierPerTier * currentDifficultyTier);
    public float SpeedMultiplier => 1f + (speedMultiplierPerTier * currentDifficultyTier);
    public float SpawnRateMultiplier => 1f + (spawnRateMultiplierPerTier * currentDifficultyTier);
    public int DifficultyTier => currentDifficultyTier;
}