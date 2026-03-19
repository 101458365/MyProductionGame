using UnityEngine;

public class ChestPickup : MonoBehaviour
{
    [Header("Item Pool")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    [Header("Drop Chances")]
    [SerializeField] private float commonChance   = 0.70f;
    [SerializeField] private float uncommonChance = 0.25f;
    // rareChance = 1 - common - uncommon = 0.05

    [Header("Visual")]
    [SerializeField] private float bobSpeed  = 2f;
    [SerializeField] private float bobHeight = 0.3f;

    private Vector3 startPosition;
    private bool isOpened = false;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!isOpened)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isOpened)
            OpenChest(collision.gameObject);
    }

    private void OpenChest(GameObject player)
    {
        isOpened = true;

        ItemData item = RollForItem();

        if (item != null)
        {
            // Route through ItemRevealUI — it handles pausing + AddItem on dismiss
            if (ItemRevealUI.Instance != null)
                ItemRevealUI.Instance.ShowItem(item, player);
            else
            {
                // Fallback: add directly if UI isn't set up yet
                PlayerStats stats = player.GetComponent<PlayerStats>();
                stats?.AddItem(item);
            }
        }

        // TODO: particle effect, sound

        Destroy(gameObject);
    }

    private ItemData RollForItem()
    {
        float roll      = Random.value;
        float rareChance = 1f - commonChance - uncommonChance;

        if (roll < rareChance && rareItems != null && rareItems.Length > 0)
            return rareItems[Random.Range(0, rareItems.Length)];

        if (roll < rareChance + uncommonChance && uncommonItems != null && uncommonItems.Length > 0)
            return uncommonItems[Random.Range(0, uncommonItems.Length)];

        if (commonItems != null && commonItems.Length > 0)
            return commonItems[Random.Range(0, commonItems.Length)];

        return null;
    }

    public void SetItemPools(ItemData[] common, ItemData[] uncommon, ItemData[] rare)
    {
        commonItems   = common;
        uncommonItems = uncommon;
        rareItems     = rare;
    }
}
