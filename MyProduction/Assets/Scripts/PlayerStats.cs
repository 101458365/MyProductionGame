using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Fallback Base Stats (used only if PlayerLevel is missing)")]
    [SerializeField] private float fallbackBaseMoveSpeed = 5f;

    private Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();

    private float totalDamageMultiplier = 1f;
    private float totalAttackSpeedMultiplier = 1f;
    private float totalMoveSpeedMultiplier = 1f;

    // Total regen as a fraction of max HP per second
    // e.g. 3 stacks at 0.02 = 0.06 = 6% max HP/sec
    private float totalRegenPercent = 0f;
    private int totalAOEStacks = 0;

    private float previousHealthBonus = 0f;

    private PlayerLevel playerLevel;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerRegen playerRegen;

    private void Awake()
    {
        playerLevel = GetComponent<PlayerLevel>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRegen = GetComponent<PlayerRegen>();
    }

    public void AddItem(ItemData item)
    {
        if (inventory.ContainsKey(item))
            inventory[item]++;
        else
            inventory[item] = 1;

        Debug.Log($"Picked up {item.itemName}! Stack: {inventory[item]}");
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        float totalHealthBonus = 0f;
        float totalDamageBonus = 0f;
        float totalAttackSpeedBonus = 0f;
        float totalMoveSpeedBonus = 0f;
        float totalRegen = 0f;
        int aoeStacks = 0;

        foreach (var kvp in inventory)
        {
            ItemData item = kvp.Key;
            int stacks = kvp.Value;

            totalDamageBonus += item.damageBonus * stacks;
            totalAttackSpeedBonus += item.attackSpeedBonus * stacks;
            totalMoveSpeedBonus += item.moveSpeedBonus * stacks;
            totalHealthBonus += item.maxHealthBonus * stacks;
            // regenPerSecond on ItemData is now treated as % per second (e.g. 0.02)
            totalRegen += item.regenPerSecond * stacks;

            if (item.isAOEItem)
                aoeStacks += stacks;
        }

        // Health delta
        float healthDelta = totalHealthBonus - previousHealthBonus;
        if (healthDelta > 0f && playerHealth != null)
            playerHealth.IncreaseMaxHealth(healthDelta);
        previousHealthBonus = totalHealthBonus;

        // Multipliers
        totalDamageMultiplier = 1f + totalDamageBonus;
        totalAttackSpeedMultiplier = 1f + totalAttackSpeedBonus;
        totalMoveSpeedMultiplier = 1f + totalMoveSpeedBonus;

        // Move speed
        float baseMoveSpeed = (playerLevel != null) ? playerLevel.BaseMoveSpeed : fallbackBaseMoveSpeed;
        playerMovement?.SetMoveSpeed(baseMoveSpeed * totalMoveSpeedMultiplier);

        // Regen — pass total percent to PlayerRegen
        totalRegenPercent = totalRegen;
        playerRegen?.SetRegenRate(totalRegenPercent);

        // AOE
        totalAOEStacks = aoeStacks;
    }

    public void RefreshStats() => RecalculateStats();

    public float DamageMultiplier => totalDamageMultiplier;
    public float AttackSpeedMultiplier => totalAttackSpeedMultiplier;
    public float RegenPercent => totalRegenPercent;
    public bool HasAOE => totalAOEStacks > 0;
    public int AOEStacks => totalAOEStacks;
    public Dictionary<ItemData, int> Inventory => inventory;
}