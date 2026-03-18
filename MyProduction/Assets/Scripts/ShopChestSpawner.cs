using UnityEngine;

public class ShopChestSpawner : MonoBehaviour
{
    public static ShopChestSpawner Instance;

    [Header("Prefab")]
    [SerializeField] private GameObject shopChestPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int chestCount = 5;
    [SerializeField] private float minDistanceBetweenChests = 10f;
    [SerializeField] private float minDistanceFromPlayer = 8f;
    [SerializeField] private int maxPlacementAttempts = 50;

    [Header("Item Pools (injected into each ShopChest)")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // Small delay to ensure ProceduralMapGenerator.Start() has fully run first.
        // If you call SpawnShopChests() directly from ProceduralMapGenerator, remove this.
        Invoke(nameof(SpawnShopChests), 0.05f);
    }

    public void SpawnShopChests()
    {
        if (shopChestPrefab == null)
        {
            Debug.LogWarning("[ShopChestSpawner] shopChestPrefab not assigned!");
            return;
        }

        if (ProceduralMapGenerator.Instance == null)
        {
            Debug.LogWarning("[ShopChestSpawner] ProceduralMapGenerator.Instance not found.");
            return;
        }

        Vector3 playerPos = Vector3.zero;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerPos = player.transform.position;

        int placed = 0;
        var placedPositions = new System.Collections.Generic.List<Vector3>();

        for (int attempt = 0; attempt < maxPlacementAttempts && placed < chestCount; attempt++)
        {
            Vector3 candidate = ProceduralMapGenerator.Instance.GetRandomWalkablePosition();

            // Must be far enough from player spawn
            if (Vector3.Distance(candidate, playerPos) < minDistanceFromPlayer)
                continue;

            // Must be far enough from other chests
            bool tooClose = false;
            foreach (var pos in placedPositions)
            {
                if (Vector3.Distance(candidate, pos) < minDistanceBetweenChests)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // Place the chest
            GameObject chest = Instantiate(shopChestPrefab, candidate, Quaternion.identity);
            ShopChest shopChest = chest.GetComponent<ShopChest>();
            shopChest?.SetItemPools(commonItems, uncommonItems, rareItems);

            placedPositions.Add(candidate);
            placed++;
        }

        Debug.Log($"[ShopChestSpawner] Placed {placed}/{chestCount} shop chests.");
    }
}
