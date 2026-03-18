using UnityEngine;
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject eliteEnemyPrefab;
    [SerializeField] [Range(0f, 1f)] private float eliteSpawnChance = 0.08f;  // 8% chance per spawn

    [SerializeField] private float initialSpawnRate = 2f;
    [SerializeField] private float minimumSpawnRate = 0.3f;
    [SerializeField] private float spawnRateDecreaseOverTime = 0.05f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20f, 20f);
    [SerializeField] private float minDistanceFromPlayer = 5f;

    [Header("Map-Aware Spawning")]
    [SerializeField] private int maxSpawnAttempts = 15;
    [SerializeField] private bool useMapBoundsForSpawning = true;

    private Transform player;
    private float currentSpawnRate;
    private float nextSpawnTime;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + currentSpawnRate;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnEnemy();

            currentSpawnRate = Mathf.Max(minimumSpawnRate, currentSpawnRate - spawnRateDecreaseOverTime);

            float adjustedRate = currentSpawnRate;
            if (DifficultyManager.Instance != null)
                adjustedRate /= DifficultyManager.Instance.SpawnRateMultiplier;

            nextSpawnTime = Time.time + adjustedRate;
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPos = FindValidSpawnPosition();
        if (spawnPos == Vector2.negativeInfinity) return;

        // Decide regular vs elite
        bool spawnElite = eliteEnemyPrefab != null && Random.value <= eliteSpawnChance;
        GameObject prefab = spawnElite ? eliteEnemyPrefab : enemyPrefab;

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    private Vector2 FindValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 candidate = GetRandomSpawnPosition();

            if (Vector2.Distance(candidate, player.position) < minDistanceFromPlayer)
                continue;

            if (useMapBoundsForSpawning && ProceduralMapGenerator.Instance != null)
                if (!ProceduralMapGenerator.Instance.IsWalkable(candidate))
                    continue;

            return candidate;
        }

        Debug.LogWarning("[EnemySpawner] Could not find valid spawn position.");
        return Vector2.negativeInfinity;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float x = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float y = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
        return (Vector2)transform.position + new Vector2(x, y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.position, minDistanceFromPlayer);
        }
    }
}
