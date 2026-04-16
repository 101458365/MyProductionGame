using UnityEngine;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private GameObject goldOrbPrefab;
    [SerializeField] private GameObject healPickupPrefab;
    [SerializeField] private GameObject magnetPickupPrefab;

    [Header("Item Pool (for chests)")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    [Header("Drop Chances - Regular Enemy")]
    [SerializeField] private float chestDropChance = 0.12f;

    [Header("Drop Chances - Elite Enemy (must add up to 1.0)")]
    [SerializeField] private float eliteChestChance = 0.40f;
    [SerializeField] private float eliteHealChance = 0.25f;
    [SerializeField] private float eliteMagnetChance = 0.15f;
    [SerializeField] private float eliteNothingChance = 0.20f;

    [Header("XP Settings")]
    [SerializeField] private int baseXPAmount = 5;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SpawnDrops(Vector3 position)
    {
        SpawnXP(position);

        if (Random.value <= chestDropChance)
            SpawnChest(position);
    }

    public void SpawnEliteDrops(Vector3 position)
    {
        // Always bonus XP and gold
        SpawnXP(position, multiplier: 3);
        SpawnGold(position, 15);

        float roll = Random.value;

        float chestThreshold = eliteChestChance;
        float healThreshold = chestThreshold + eliteHealChance;
        float magnetThreshold = healThreshold + eliteMagnetChance;
        float nothingThreshold = magnetThreshold + eliteNothingChance;

        if (roll < chestThreshold)
        {
            SpawnChest(position);
            Debug.Log("[DropManager] Elite dropped: Chest");
        }
        else if (roll < healThreshold)
        {
            SpawnHeal(position);
            Debug.Log("[DropManager] Elite dropped: Heal");
        }
        else if (roll < magnetThreshold)
        {
            SpawnMagnet(position);
            Debug.Log("[DropManager] Elite dropped: Magnet");
        }
        else if (roll < nothingThreshold)
        {
            Debug.Log("[DropManager] Elite dropped: Nothing");
        }
        else
        {
            Debug.LogWarning("[DropManager] Drop chances do not sum to 1!");
        }
    }

    public void SpawnGold(Vector3 position, int totalAmount)
    {
        if (goldOrbPrefab == null) return;

        int coinCount = Mathf.Clamp(totalAmount / 3, 1, 5);
        int remaining = totalAmount;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 offset = position + (Vector3)(Random.insideUnitCircle * 0.5f);
            GameObject coin = Instantiate(goldOrbPrefab, offset, Quaternion.identity);
            GoldPickup pickup = coin.GetComponent<GoldPickup>();

            int value = (i == coinCount - 1) ? remaining : Mathf.Max(1, totalAmount / coinCount);
            remaining -= value;
            pickup?.SetGoldAmount(Mathf.Max(1, value));
        }
    }

    private void SpawnXP(Vector3 position, int multiplier = 1)
    {
        if (xpOrbPrefab == null) return;
        GameObject xpOrb = Instantiate(xpOrbPrefab, position, Quaternion.identity);
        xpOrb.GetComponent<XPPickup>()?.SetXPAmount(baseXPAmount * multiplier);
    }

    private void SpawnChest(Vector3 position)
    {
        if (chestPrefab == null) return;
        GameObject chest = Instantiate(chestPrefab, position, Quaternion.identity);
        chest.GetComponent<ChestPickup>()?.SetItemPools(commonItems, uncommonItems, rareItems);
    }

    private void SpawnHeal(Vector3 position)
    {
        if (healPickupPrefab == null)
        {
            Debug.LogWarning("[DropManager] healPickupPrefab not assigned!");
            return;
        }
        Instantiate(healPickupPrefab, position, Quaternion.identity);
    }

    private void SpawnMagnet(Vector3 position)
    {
        if (magnetPickupPrefab == null)
        {
            Debug.LogWarning("[DropManager] magnetPickupPrefab not assigned!");
            return;
        }
        Instantiate(magnetPickupPrefab, position, Quaternion.identity);
    }
}