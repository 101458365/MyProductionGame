using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float initialSpawnRate = 2f;
    [SerializeField] private float minimumSpawnRate = 0.3f;
    [SerializeField] private float spawnRateDecreaseOverTime = 0.05f;

    [Header("Spawn Area")]
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(20f, 20f);
    [SerializeField] private float minDistanceFromPlayer = 5f;

    [Header("Map-Aware Spawning")]
    [SerializeField] private int maxSpawnAttempts = 15;      // More attempts since water is now invalid too
    [SerializeField] private bool useMapBoundsForSpawning = true; // If false, falls back to old behavior

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

            if (DifficultyManager.Instance != null)
            {
                float adjustedSpawnRate = currentSpawnRate / DifficultyManager.Instance.SpawnRateMultiplier;
                nextSpawnTime = Time.time + adjustedSpawnRate;
            }
            else
            {
                nextSpawnTime = Time.time + currentSpawnRate;
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition != Vector2.negativeInfinity)
        {
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private Vector2 FindValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 candidate = GetRandomSpawnPosition();

            // Must be far enough from player
            if (Vector2.Distance(candidate, player.position) < minDistanceFromPlayer)
                continue;

            // Must be on walkable land (checks ProceduralMapGenerator if available)
            if (useMapBoundsForSpawning && ProceduralMapGenerator.Instance != null)
            {
                if (!ProceduralMapGenerator.Instance.IsWalkable(candidate))
                    continue;
            }

            return candidate;
        }

        Debug.LogWarning("[EnemySpawner] Could not find valid spawn position after max attempts.");
        return Vector2.negativeInfinity;
    }

    private Vector2 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
        float randomY = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);
        return (Vector2)transform.position + new Vector2(randomX, randomY);
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