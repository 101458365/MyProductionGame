using UnityEngine;
using System.Collections.Generic;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float baseAttackSpeed = 1f;
    [SerializeField] private float baseMoveSpeed = 5f;

    // Item inventory (item -> stack count)
    private Dictionary<ItemData, int> inventory = new Dictionary<ItemData, int>();

    // Calculated stats
    private float totalDamageMultiplier = 1f;
    private float totalAttackSpeedMultiplier = 1f;
    private float totalMoveSpeedMultiplier = 1f;

    // Track previous bonuses to avoid compounding
    private float previousHealthBonus = 0f;
    private float previousDamageBonus = 0f;
    private float previousAttackSpeedBonus = 0f;
    private float previousMoveSpeedBonus = 0f;

    public void AddItem(ItemData item)
    {
        if (inventory.ContainsKey(item))
        {
            inventory[item]++;
        }
        else
        {
            inventory[item] = 1;
        }

        RecalculateStats();

        Debug.Log($"Picked up {item.itemName}! Stack: {inventory[item]}");
    }

    private void RecalculateStats()
    {
        // Calculate total bonuses from all items
        float totalHealthBonus = 0f;
        float totalDamageBonus = 0f;
        float totalAttackSpeedBonus = 0f;
        float totalMoveSpeedBonus = 0f;

        // Sum up all item bonuses
        foreach (var kvp in inventory)
        {
            ItemData item = kvp.Key;
            int stacks = kvp.Value;

            totalDamageBonus += item.damageBonus * stacks;
            totalAttackSpeedBonus += item.attackSpeedBonus * stacks;
            totalMoveSpeedBonus += item.moveSpeedBonus * stacks;
            totalHealthBonus += item.maxHealthBonus * stacks;
        }

        // Apply ONLY the new bonuses (difference from previous)

        // Health
        float healthDifference = totalHealthBonus - previousHealthBonus;
        if (healthDifference > 0)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.IncreaseMaxHealth(healthDifference);
            }
        }
        previousHealthBonus = totalHealthBonus;

        // Move Speed - just set the multiplier directly
        totalMoveSpeedMultiplier = 1f + totalMoveSpeedBonus;
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMoveSpeed(baseMoveSpeed * totalMoveSpeedMultiplier);
        }

        // Damage and Attack Speed - store as multipliers
        totalDamageMultiplier = 1f + totalDamageBonus;
        totalAttackSpeedMultiplier = 1f + totalAttackSpeedBonus;
    }

    // Getters
    public float DamageMultiplier => totalDamageMultiplier;
    public float AttackSpeedMultiplier => totalAttackSpeedMultiplier;
    public Dictionary<ItemData, int> Inventory => inventory;
}