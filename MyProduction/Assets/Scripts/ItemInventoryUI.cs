using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ItemInventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform itemContainer;

    [Header("Slot Settings")]
    [SerializeField] private int slotSize = 48;
    [SerializeField] private bool showCountWhenOne = false;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color uncommonColor = new Color(0.10f, 0.75f, 0.10f);
    [SerializeField] private Color rareColor = new Color(0.85f, 0.20f, 0.20f);

    private PlayerStats playerStats;
    private Dictionary<ItemData, ItemSlotUI> activeSlots = new Dictionary<ItemData, ItemSlotUI>();

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerStats = player.GetComponent<PlayerStats>();

        if (playerStats == null)
            Debug.LogWarning("[ItemInventoryUI] Could not find PlayerStats on Player.");
    }

    private void Update()
    {
        if (playerStats == null) return;
        RefreshUI();
    }

    private void RefreshUI()
    {
        var inventory = playerStats.Inventory;

        foreach (var kvp in inventory)
        {
            ItemData item = kvp.Key;
            int stacks = kvp.Value;

            if (activeSlots.ContainsKey(item))
            {
                activeSlots[item].SetCount(stacks, showCountWhenOne);
            }
            else
            {
                GameObject slotObj = Instantiate(itemSlotPrefab, itemContainer);
                ItemSlotUI slot = slotObj.GetComponent<ItemSlotUI>();

                if (slot != null)
                {
                    slot.Initialize(item.icon, stacks, GetRarityColor(item.rarity), slotSize, showCountWhenOne);
                    activeSlots[item] = slot;
                }
            }
        }

        // Clean up any slots for items no longer in inventory
        List<ItemData> toRemove = new List<ItemData>();
        foreach (var kvp in activeSlots)
        {
            if (!inventory.ContainsKey(kvp.Key))
            {
                Destroy(kvp.Value.gameObject);
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var item in toRemove)
            activeSlots.Remove(item);
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