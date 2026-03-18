using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Updated ShopChest — routes item reveal through ItemRevealUI.
/// Drop this in to REPLACE your existing ShopChest.cs
/// </summary>
public class ShopChest : MonoBehaviour
{
    [Header("Cost")]
    [SerializeField] private int cost = 20;
    [SerializeField] private int costVariance = 10;

    [Header("Item Pool")]
    [SerializeField] private ItemData[] commonItems;
    [SerializeField] private ItemData[] uncommonItems;
    [SerializeField] private ItemData[] rareItems;

    [Header("Drop Chances")]
    [SerializeField] private float commonChance   = 0.70f;
    [SerializeField] private float uncommonChance = 0.25f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject pricePanel;

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private int flashCount = 3;

    [Header("Visual Bobbing")]
    [SerializeField] private float bobSpeed  = 2f;
    [SerializeField] private float bobHeight = 0.15f;

    private int finalCost;
    private bool playerNearby = false;
    private bool isOpened     = false;
    private GameObject playerObj;
    private Vector3 startPosition;
    private Coroutine flashCoroutine;

    private void Start()
    {
        startPosition = transform.position;
        finalCost     = Mathf.Max(5, cost + Random.Range(-costVariance, costVariance + 1));
        HidePrice();
    }

    private void Update()
    {
        if (isOpened) return;

        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (playerNearby && Keyboard.current.eKey.wasPressedThisFrame)
            TryOpen();
    }

    private void TryOpen()
    {
        if (GoldManager.Instance == null) return;

        if (GoldManager.Instance.SpendGold(finalCost))
            OpenChest();
        else
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashPriceRed());
        }
    }

    private void OpenChest()
    {
        isOpened = true;
        HidePrice();

        ItemData item = RollForItem();

        if (item != null && playerObj != null)
        {
            // Route through ItemRevealUI — it handles pausing + AddItem on dismiss
            if (ItemRevealUI.Instance != null)
                ItemRevealUI.Instance.ShowItem(item, playerObj);
            else
            {
                PlayerStats stats = playerObj.GetComponent<PlayerStats>();
                stats?.AddItem(item);
            }
        }

        Destroy(gameObject);
    }

    private ItemData RollForItem()
    {
        float roll       = Random.value;
        float rareChance = 1f - commonChance - uncommonChance;

        if (roll < rareChance && rareItems != null && rareItems.Length > 0)
            return rareItems[Random.Range(0, rareItems.Length)];

        if (roll < rareChance + uncommonChance && uncommonItems != null && uncommonItems.Length > 0)
            return uncommonItems[Random.Range(0, uncommonItems.Length)];

        if (commonItems != null && commonItems.Length > 0)
            return commonItems[Random.Range(0, commonItems.Length)];

        return null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerObj     = collision.gameObject;
        playerNearby  = true;
        ShowPrice();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerNearby = false;
        HidePrice();
    }

    private void ShowPrice()
    {
        if (priceText != null)
        {
            priceText.text  = $"{finalCost}g  [E]";
            priceText.color = Color.white;
            priceText.gameObject.SetActive(true);
        }
        if (pricePanel != null) pricePanel.SetActive(true);
    }

    private void HidePrice()
    {
        if (priceText != null) priceText.gameObject.SetActive(false);
        if (pricePanel != null) pricePanel.SetActive(false);
    }

    private System.Collections.IEnumerator FlashPriceRed()
    {
        if (priceText == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            priceText.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            priceText.color = Color.white;
            yield return new WaitForSeconds(flashDuration);
        }
    }

    public void SetItemPools(ItemData[] common, ItemData[] uncommon, ItemData[] rare)
    {
        commonItems   = common;
        uncommonItems = uncommon;
        rareItems     = rare;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
            Gizmos.DrawWireSphere(transform.position, col.radius);
    }
}
