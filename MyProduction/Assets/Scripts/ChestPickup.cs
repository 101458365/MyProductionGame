using UnityEngine;

public class ChestPickup : MonoBehaviour
{
    [Header("Item Pool")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    [Header("Drop Chances")]
    [SerializeField] private float commonChance = 0.70f;  // 70%
    [SerializeField] private float uncommonChance = 0.25f; // 25%
    [SerializeField] private float rareChance = 0.05f;    // 5%

    [Header("Visual")]
    [SerializeField] private float bobSpeed = 2f;
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
            // Bob up and down
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isOpened)
        {
            OpenChest(collision.gameObject);
        }
    }

    private void OpenChest(GameObject player)
    {
        isOpened = true;

        // Get random item
        ItemData item = RollForItem();

        if (item != null)
        {
            PlayerStats stats = player.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddItem(item);
            }
        }

        // TODO: Particle effect, sound

        Destroy(gameObject);
    }

    private ItemData RollForItem()
    {
        float roll = Random.value;

        if (roll <= rareChance && rareItems.Length > 0)
        {
            return rareItems[Random.Range(0, rareItems.Length)];
        }
        else if (roll <= rareChance + uncommonChance && uncommonItems.Length > 0)
        {
            return uncommonItems[Random.Range(0, uncommonItems.Length)];
        }
        else if (commonItems.Length > 0)
        {
            return commonItems[Random.Range(0, commonItems.Length)];
        }

        return null;
    }

    // Allow setting items from DropManager
    public void SetItemPools(ItemData[] common, ItemData[] uncommon, ItemData[] rare)
    {
        commonItems = common;
        uncommonItems = uncommon;
        rareItems = rare;
    }
}