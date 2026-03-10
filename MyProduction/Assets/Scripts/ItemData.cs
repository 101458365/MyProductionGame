using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;

    [Header("Stats Per Stack")]
    public float damageBonus = 0f;
    public float attackSpeedBonus = 0f;
    public float maxHealthBonus = 0f;
    public float moveSpeedBonus = 0f;

    [TextArea(3, 5)]
    public string description;
}

public enum ItemRarity
{
    Common,    // White - 70% drop rate
    Uncommon,  // Green - 25% drop rate
    Rare       // Red - 5% drop rate
}