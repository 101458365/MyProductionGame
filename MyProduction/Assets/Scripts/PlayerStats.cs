using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    // These are now only fallbacks if PlayerLevel isn't present.
    // PlayerLevel is the single source of truth for base stats.
    [Header("Fallback Base Stats (used only if PlayerLevel is missing)")]
    [SerializeField] private float fallbackBaseDamage = 1f;
    [SerializeField] private float fallbackBaseAttackSpeed = 1f;
    [SerializeField] private float fallbackBaseMoveSpeed = 5f;

    // Item inventory: item → stack count
    private Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();

    // Calculated multipliers from items only (not level bonuses)
    private float totalDamageMultiplier = 1f;
    private float totalAttackSpeedMultiplier = 1f;
    private float totalMoveSpeedMultiplier = 1f;

    // Track previous health bonus so we only apply the delta on each recalc
    private float previousHealthBonus = 0f;

    private PlayerLevel playerLevel;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerLevel = GetComponent<PlayerLevel>();
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
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

        foreach (var kvp in inventory)
        {
            ItemData item = kvp.Key;
            int stacks = kvp.Value;

            totalDamageBonus += item.damageBonus * stacks;
            totalAttackSpeedBonus += item.attackSpeedBonus * stacks;
            totalMoveSpeedBonus += item.moveSpeedBonus * stacks;
            totalHealthBonus += item.maxHealthBonus * stacks;
        }

        float healthDelta = totalHealthBonus - previousHealthBonus;
        if (healthDelta > 0f && playerHealth != null)
            playerHealth.IncreaseMaxHealth(healthDelta);
        previousHealthBonus = totalHealthBonus;

        totalDamageMultiplier = 1f + totalDamageBonus;
        totalAttackSpeedMultiplier = 1f + totalAttackSpeedBonus;
        totalMoveSpeedMultiplier = 1f + totalMoveSpeedBonus;

        // ── Apply move speed: BASE comes from PlayerLevel, then multiply ─
        // This was the bug: previously used a hardcoded 5f here, which
        // wiped out any speed gained from level-up choices.
        float baseMoveSpeed = (playerLevel != null)
            ? playerLevel.BaseMoveSpeed
            : fallbackBaseMoveSpeed;

        if (playerMovement != null)
            playerMovement.SetMoveSpeed(baseMoveSpeed * totalMoveSpeedMultiplier);

        // Damage and attack speed multipliers are read on-demand by
        // PlayerShooting via the getters below, so no push needed here.
    }

    
    public void RefreshStats() => RecalculateStats();

    public float DamageMultiplier => totalDamageMultiplier;
    public float AttackSpeedMultiplier => totalAttackSpeedMultiplier;
    public Dictionary<ItemData, int> Inventory => inventory;
}
