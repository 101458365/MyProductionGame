using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class ItemRevealUI : MonoBehaviour
{
    public static ItemRevealUI Instance;

    [Header("Panel")]
    [SerializeField] private GameObject revealPanel;

    [Header("Item Display")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI clickToContinueText;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color uncommonColor = new Color(0.10f, 0.85f, 0.10f);
    [SerializeField] private Color rareColor = new Color(0.95f, 0.20f, 0.20f);

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 2f;   // speed of the "click to continue" pulse

    private bool isShowing = false;
    private ItemData pendingItem = null;
    private GameObject pendingPlayer = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (revealPanel != null)
            revealPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isShowing) return;

        // Pulse the click to continue text
        if (clickToContinueText != null)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.unscaledTime * pulseSpeed));
            Color c = clickToContinueText.color;
            clickToContinueText.color = new Color(c.r, c.g, c.b, alpha);
        }

        // Any click or E key dismisses the panel
        if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            Dismiss();
    }

    public void ShowItem(ItemData item, GameObject player)
    {
        if (item == null) return;

        pendingItem = item;
        pendingPlayer = player;

        // Populate UI
        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = item.icon != null;
        }

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (rarityText != null)
        {
            rarityText.text = item.rarity.ToString();
            rarityText.color = GetRarityColor(item.rarity);
        }

        if (descriptionText != null)
            descriptionText.text = item.description;

        if (clickToContinueText != null)
            clickToContinueText.text = "Click anywhere to continue";

        // Show panel and pause
        if (revealPanel != null)
            revealPanel.SetActive(true);

        isShowing = true;
        Time.timeScale = 0f;
    }

    private void Dismiss()
    {
        // Add the item to inventory now that the player has seen it
        if (pendingItem != null && pendingPlayer != null)
        {
            PlayerStats stats = pendingPlayer.GetComponent<PlayerStats>();
            stats?.AddItem(pendingItem);
        }

        pendingItem = null;
        pendingPlayer = null;
        isShowing = false;

        if (revealPanel != null)
            revealPanel.SetActive(false);

        // Resume — but only if nothing else has paused the game
        // (LevelUpUI also pauses, so we don't blindly set timeScale to 1)
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Uncommon: return uncommonColor;
            case ItemRarity.Rare: return rareColor;
            default: return commonColor;
        }
    }
}