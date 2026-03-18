using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    [SerializeField] private int mapWidth = 120;
    [SerializeField] private int mapHeight = 120;

    [Header("Noise Settings")]
    [SerializeField] private float noiseScale = 18f;
    // Lower waterThreshold = less water. 0.35 gives ~70% land coverage.
    [SerializeField][Range(0f, 1f)] private float waterThreshold = 0.35f;
    [SerializeField] private int seed = 0;           // 0 = random each run
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float lacunarity = 2f;

    [Header("Cellular Automata Smoothing")]
    [SerializeField] private int smoothingPasses = 3;
    [SerializeField] private int neighboursRequiredForWater = 5;

    [Header("Island Shape")]
    // Lower falloffStrength = bigger land mass, less water at edges.
    // 0.5 = mostly land with water only near corners/edges.
    // 1.2 = small island surrounded by water (old default).
    [SerializeField][Range(0.2f, 1.5f)] private float falloffStrength = 0.55f;
    // Higher falloffCurve = sharper edge dropoff (more circular island shape).
    [SerializeField][Range(1f, 4f)] private float falloffCurve = 2.2f;

    [Header("Tilemaps")]
    [SerializeField] private Tilemap groundTilemap;
    [SerializeField] private Tilemap waterTilemap;
    [SerializeField] private Tilemap decorationTilemap;  // Leave unassigned for now

    [Header("Grass Tiles (pick randomly)")]
    [SerializeField] private TileBase[] grassTiles;

    [Header("Sand Tiles (pick randomly)")]
    [SerializeField] private TileBase[] sandTiles;

    [Header("Water Tiles (pick randomly)")]
    [SerializeField] private TileBase[] waterTiles;

    [Header("Decoration Tiles (leave empty for now)")]
    [SerializeField] private TileBase[] decorationTiles;
    [SerializeField][Range(0f, 1f)] private float decorationDensity = 0.08f;

    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnParent;
    // How many tiles away from water the player must be to spawn.
    // Increase this if the player is still spawning too close to water.
    [SerializeField] private int safeSpawnWaterDistance = 3;

    // Internal map: true = water/impassable, false = land/walkable
    private bool[,] waterMap;
    private int actualSeed;

    public static ProceduralMapGenerator Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateMap();
    }

    public void RegenerateMap()
    {
        ClearMap();
        GenerateMap();
    }

    public bool IsWalkable(Vector3 worldPosition)
    {
        Vector3Int cell = groundTilemap.WorldToCell(worldPosition);
        int x = cell.x + mapWidth / 2;
        int y = cell.y + mapHeight / 2;

        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
            return false;

        return !waterMap[x, y];
    }

    public Vector3 GetRandomWalkablePosition()
    {
        List<Vector2Int> walkableCells = new List<Vector2Int>();

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (!waterMap[x, y])
                    walkableCells.Add(new Vector2Int(x, y));

        if (walkableCells.Count == 0)
        {
            Debug.LogWarning("[MapGenerator] No walkable cells found!");
            return Vector3.zero;
        }

        Vector2Int randomCell = walkableCells[Random.Range(0, walkableCells.Count)];
        return CellToWorld(randomCell.x, randomCell.y);
    }

    private void GenerateMap()
    {
        actualSeed = (seed == 0) ? Random.Range(1, 999999) : seed;
        Random.InitState(actualSeed);
        Debug.Log($"[MapGenerator] Seed: {actualSeed}");

        waterMap = GenerateNoiseMap();

        for (int i = 0; i < smoothingPasses; i++)
            waterMap = SmoothMap(waterMap);

        EnsureConnectedLand();
        PaintTiles();

        if (decorationTilemap != null && decorationTiles != null && decorationTiles.Length > 0)
            PlaceDecorations();

        SpawnPlayer();

        Debug.Log("[MapGenerator] Done.");
    }

    private void ClearMap()
    {
        groundTilemap?.ClearAllTiles();
        waterTilemap?.ClearAllTiles();
        decorationTilemap?.ClearAllTiles();
    }

    private bool[,] GenerateNoiseMap()
    {
        bool[,] map = new bool[mapWidth, mapHeight];

        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
            octaveOffsets[i] = new Vector2(Random.Range(-10000f, 10000f), Random.Range(-10000f, 10000f));

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseValue = 0f;
                float maxValue = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x + octaveOffsets[o].x) / noiseScale * frequency;
                    float sampleY = (y + octaveOffsets[o].y) / noiseScale * frequency;
                    noiseValue += Mathf.PerlinNoise(sampleX, sampleY) * amplitude;
                    maxValue += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                noiseValue /= maxValue;

                float falloff = IslandFalloff(x, y);
                noiseValue = Mathf.Clamp01(noiseValue - falloff);

                map[x, y] = noiseValue < waterThreshold;
            }
        }

        return map;
    }

    private float IslandFalloff(int x, int y)
    {
        float nx = (float)x / mapWidth * 2f - 1f;
        float ny = (float)y / mapHeight * 2f - 1f;
        float dist = Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny));
        return Mathf.Pow(dist, falloffCurve) * falloffStrength;
    }

    private bool[,] SmoothMap(bool[,] map)
    {
        bool[,] newMap = new bool[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int waterNeighbours = CountWaterNeighbours(map, x, y);

                if (waterNeighbours > neighboursRequiredForWater)
                    newMap[x, y] = true;
                else if (waterNeighbours < neighboursRequiredForWater)
                    newMap[x, y] = false;
                else
                    newMap[x, y] = map[x, y];
            }
        }

        return newMap;
    }

    private int CountWaterNeighbours(bool[,] map, int x, int y)
    {
        int count = 0;
        for (int nx = x - 1; nx <= x + 1; nx++)
        {
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx == x && ny == y) continue;

                if (nx < 0 || nx >= mapWidth || ny < 0 || ny >= mapHeight)
                    count++;
                else if (map[nx, ny])
                    count++;
            }
        }
        return count;
    }

    private void EnsureConnectedLand()
    {
        bool[,] visited = new bool[mapWidth, mapHeight];
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (!visited[x, y] && !waterMap[x, y])
                    regions.Add(FloodFillRegion(x, y, visited));

        if (regions.Count == 0)
        {
            Debug.LogWarning("[MapGenerator] No land found! Lower waterThreshold.");
            return;
        }

        List<Vector2Int> largestRegion = regions[0];
        foreach (var region in regions)
            if (region.Count > largestRegion.Count)
                largestRegion = region;

        HashSet<Vector2Int> keepSet = new HashSet<Vector2Int>(largestRegion);
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                if (!waterMap[x, y] && !keepSet.Contains(new Vector2Int(x, y)))
                    waterMap[x, y] = true;

        Debug.Log($"[MapGenerator] Land tiles: {largestRegion.Count}. Removed {regions.Count - 1} islands.");
    }

    private List<Vector2Int> FloodFillRegion(int startX, int startY, bool[,] visited)
    {
        List<Vector2Int> region = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            region.Add(cell);

            Vector2Int[] neighbours = {
                new Vector2Int(cell.x + 1, cell.y),
                new Vector2Int(cell.x - 1, cell.y),
                new Vector2Int(cell.x, cell.y + 1),
                new Vector2Int(cell.x, cell.y - 1),
            };

            foreach (var n in neighbours)
            {
                if (n.x >= 0 && n.x < mapWidth && n.y >= 0 && n.y < mapHeight
                    && !visited[n.x, n.y] && !waterMap[n.x, n.y])
                {
                    visited[n.x, n.y] = true;
                    queue.Enqueue(n);
                }
            }
        }

        return region;
    }

    private void PaintTiles()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePos = WorldToCellPosition(x, y);

                if (waterMap[x, y])
                {
                    TileBase tile = PickRandom(waterTiles);
                    if (waterTilemap != null && tile != null)
                        waterTilemap.SetTile(tilePos, tile);
                }
                else
                {
                    TileBase tile = IsAdjacentToWater(x, y) ? PickRandom(sandTiles) : PickRandom(grassTiles);
                    if (groundTilemap != null && tile != null)
                        groundTilemap.SetTile(tilePos, tile);
                }
            }
        }
    }

    private bool IsAdjacentToWater(int x, int y)
    {
        for (int nx = x - 1; nx <= x + 1; nx++)
            for (int ny = y - 1; ny <= y + 1; ny++)
                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight && waterMap[nx, ny])
                    return true;
        return false;
    }

    private bool IsSafeFromWater(int x, int y, int distance)
    {
        for (int nx = x - distance; nx <= x + distance; nx++)
            for (int ny = y - distance; ny <= y + distance; ny++)
                if (nx >= 0 && nx < mapWidth && ny >= 0 && ny < mapHeight && waterMap[nx, ny])
                    return false;
        return true;
    }

    private void PlaceDecorations()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (!waterMap[x, y] && !IsAdjacentToWater(x, y) && Random.value < decorationDensity)
                {
                    TileBase deco = PickRandom(decorationTiles);
                    if (deco != null)
                        decorationTilemap.SetTile(WorldToCellPosition(x, y), deco);
                }
            }
        }
    }

    private void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        Vector3 spawnPos = FindSafeSpawnPosition();

        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            existingPlayer.transform.position = spawnPos;
            Debug.Log($"[MapGenerator] Moved player to {spawnPos}");
        }
        else
        {
            Instantiate(playerPrefab, spawnPos, Quaternion.identity, playerSpawnParent);
            Debug.Log($"[MapGenerator] Spawned player at {spawnPos}");
        }
    }

    private Vector3 FindSafeSpawnPosition()
    {
        int centerX = mapWidth / 2;
        int centerY = mapHeight / 2;

        // First pass: find a tile at least safeSpawnWaterDistance tiles from any water.
        // Searches outward from center so the player always starts near the middle of the map.
        for (int radius = 0; radius < Mathf.Max(mapWidth, mapHeight); radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    // Only check the outer ring at this radius
                    if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;

                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) continue;
                    if (waterMap[x, y]) continue;

                    if (IsSafeFromWater(x, y, safeSpawnWaterDistance))
                        return CellToWorld(x, y);
                }
            }
        }

        // Fallback: no tile met the distance requirement (very small map / high water).
        // Just find the nearest land tile to center.
        Debug.LogWarning($"[MapGenerator] No tile found {safeSpawnWaterDistance} tiles from water. Using nearest land tile.");
        for (int radius = 0; radius < Mathf.Max(mapWidth, mapHeight); radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;

                    if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight) continue;
                    if (!waterMap[x, y])
                        return CellToWorld(x, y);
                }
            }
        }

        Debug.LogError("[MapGenerator] No land tiles exist! Check waterThreshold.");
        return Vector3.zero;
    }

    private TileBase PickRandom(TileBase[] tiles)
    {
        if (tiles == null || tiles.Length == 0) return null;
        return tiles[Random.Range(0, tiles.Length)];
    }

    private Vector3Int WorldToCellPosition(int x, int y)
    {
        return new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0);
    }

    private Vector3 CellToWorld(int x, int y)
    {
        Vector3Int cellPos = WorldToCellPosition(x, y);
        return groundTilemap.CellToWorld(cellPos) + groundTilemap.cellSize / 2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(mapWidth, mapHeight, 0));
    }
}