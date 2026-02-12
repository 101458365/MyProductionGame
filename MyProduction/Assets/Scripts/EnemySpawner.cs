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
            nextSpawnTime = Time.time + currentSpawnRate;
        }
    }

    private void SpawnEnemy()
    {
        Vector2 spawnPosition;
        int attempts = 0;

        do
        {
            spawnPosition = GetRandomSpawnPosition();
            attempts++;

            if (attempts > 10) break;

        } while (Vector2.Distance(spawnPosition, player.position) < minDistanceFromPlayer);

        Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
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