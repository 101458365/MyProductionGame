using UnityEngine;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject xpOrbPrefab;
    [SerializeField] private GameObject goldOrbPrefab; 

    [Header("Item Pool (for chests)")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    [Header("Drop Chances")]
    [SerializeField] private float chestDropChance = 0.12f;

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
        SpawnChest(position);
        SpawnXP(position, multiplier: 3);
    }

    public void SpawnGold(Vector3 position, int totalAmount)
    {
        if (goldOrbPrefab == null) return;

        // Scatter coins so they don't stack on one spot
        int coinCount = Mathf.Clamp(totalAmount / 3, 1, 5);
        int remaining = totalAmount;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 offset = position + (Vector3)(Random.insideUnitCircle * 0.5f);
            GameObject coin = Instantiate(goldOrbPrefab, offset, Quaternion.identity);
            GoldPickup pickup = coin.GetComponent<GoldPickup>();

            // Last coin gets whatever is left over
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
}
