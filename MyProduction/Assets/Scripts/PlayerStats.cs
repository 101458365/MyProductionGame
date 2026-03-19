using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Fallback Base Stats (used only if PlayerLevel is missing)")]
    [SerializeField] private float fallbackBaseMoveSpeed = 5f;

    private Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();

    // Multipliers from items
    private float totalDamageMultiplier     = 1f;
    private float totalAttackSpeedMultiplier = 1f;
    private float totalMoveSpeedMultiplier  = 1f;

    // Additive stats from items
    private float totalRegenPerSecond = 0f;  // total HP/sec from all regen items
    private int   totalAOEStacks      = 0;   // how many AOE items the player has

    private float previousHealthBonus = 0f;

    private PlayerLevel    playerLevel;
    private PlayerHealth   playerHealth;
    private PlayerMovement playerMovement;
    private PlayerRegen    playerRegen;

    private void Awake()
    {
        playerLevel    = GetComponent<PlayerLevel>();
        playerHealth   = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRegen    = GetComponent<PlayerRegen>();
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
        float totalHealthBonus      = 0f;
        float totalDamageBonus      = 0f;
        float totalAttackSpeedBonus = 0f;
        float totalMoveSpeedBonus   = 0f;
        float totalRegen            = 0f;
        int   aoeStacks             = 0;

        foreach (var kvp in inventory)
        {
            ItemData item  = kvp.Key;
            int      stacks = kvp.Value;

            totalDamageBonus      += item.damageBonus      * stacks;
            totalAttackSpeedBonus += item.attackSpeedBonus * stacks;
            totalMoveSpeedBonus   += item.moveSpeedBonus   * stacks;
            totalHealthBonus      += item.maxHealthBonus   * stacks;
            totalRegen            += item.regenPerSecond   * stacks;

            if (item.isAOEItem)
                aoeStacks += stacks;
        }

        float healthDelta = totalHealthBonus - previousHealthBonus;
        if (healthDelta > 0f && playerHealth != null)
            playerHealth.IncreaseMaxHealth(healthDelta);
        previousHealthBonus = totalHealthBonus;

        totalDamageMultiplier      = 1f + totalDamageBonus;
        totalAttackSpeedMultiplier = 1f + totalAttackSpeedBonus;
        totalMoveSpeedMultiplier   = 1f + totalMoveSpeedBonus;

        float baseMoveSpeed = (playerLevel != null)
            ? playerLevel.BaseMoveSpeed
            : fallbackBaseMoveSpeed;

        playerMovement?.SetMoveSpeed(baseMoveSpeed * totalMoveSpeedMultiplier);

        totalRegenPerSecond = totalRegen;
        playerRegen?.SetRegenRate(totalRegenPerSecond);

        totalAOEStacks = aoeStacks;
    }

    public void RefreshStats() => RecalculateStats();

    // ── Getters ───────────────────────────────────────────────────
    public float DamageMultiplier       => totalDamageMultiplier;
    public float AttackSpeedMultiplier  => totalAttackSpeedMultiplier;
    public float RegenPerSecond         => totalRegenPerSecond;
    public bool  HasAOE                 => totalAOEStacks > 0;
    public int   AOEStacks              => totalAOEStacks;
    public Dictionary<ItemData, int> Inventory => inventory;
}
