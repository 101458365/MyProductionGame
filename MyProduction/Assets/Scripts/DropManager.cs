using UnityEngine;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject xpOrbPrefab;

    [Header("Item Pool (for chests)")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    [Header("Drop Chances")]
    [SerializeField] private float chestDropChance = 0.15f; // 15% chance for chest

    [Header("XP Settings")]
    [SerializeField] private int baseXPAmount = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void SpawnDrops(Vector3 position)
    {
        // Always spawn XP
        SpawnXP(position);

        // Chance to spawn chest
        if (Random.value <= chestDropChance)
        {
            SpawnChest(position);
        }
    }

    private void SpawnXP(Vector3 position)
    {
        if (xpOrbPrefab != null)
        {
            GameObject xpOrb = Instantiate(xpOrbPrefab, position, Quaternion.identity);
            XPPickup pickup = xpOrb.GetComponent<XPPickup>();

            if (pickup != null)
            {
                pickup.SetXPAmount(baseXPAmount);
            }
        }
    }

    private void SpawnChest(Vector3 position)
    {
        if (chestPrefab != null)
        {
            GameObject chest = Instantiate(chestPrefab, position, Quaternion.identity);
            ChestPickup chestPickup = chest.GetComponent<ChestPickup>();

            if (chestPickup != null)
            {
                chestPickup.SetItemPools(commonItems, uncommonItems, rareItems);
            }
        }
    }
}