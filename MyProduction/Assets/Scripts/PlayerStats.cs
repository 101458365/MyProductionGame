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

    // Track previous health bonus to avoid compounding
    private float previousHealthBonus = 0f;

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
        // Reset multipliers
        totalDamageMultiplier = 1f;
        totalAttackSpeedMultiplier = 1f;
        totalMoveSpeedMultiplier = 1f;
        float totalHealthBonus = 0f;

        // Apply all items
        foreach (var kvp in inventory)
        {
            ItemData item = kvp.Key;
            int stacks = kvp.Value;

            totalDamageMultiplier += item.damageBonus * stacks;
            totalAttackSpeedMultiplier += item.attackSpeedBonus * stacks;
            totalMoveSpeedMultiplier += item.moveSpeedBonus * stacks;
            totalHealthBonus += item.maxHealthBonus * stacks;
        }

        // Apply health bonus (only the DIFFERENCE from last time)
        float healthDifference = totalHealthBonus - previousHealthBonus;
        if (healthDifference > 0)
        {
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.IncreaseMaxHealth(healthDifference); // Only add the NEW health
            }
        }
        previousHealthBonus = totalHealthBonus; // Update tracking

        // Apply move speed
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMoveSpeed(baseMoveSpeed * totalMoveSpeedMultiplier);
        }
    }

    // Getters
    public float DamageMultiplier => totalDamageMultiplier;
    public float AttackSpeedMultiplier => totalAttackSpeedMultiplier;
    public Dictionary<ItemData, int> Inventory => inventory;
}