using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    public Sprite icon;
    public ItemRarity rarity;

    [Header("Stats Per Stack")]
    public float damageBonus      = 0f;
    public float attackSpeedBonus = 0f;
    public float maxHealthBonus   = 0f;
    public float moveSpeedBonus   = 0f;
    public float regenPerSecond   = 0f;  // HP regen added per stack (e.g. 1 = +1 HP/sec per stack)
    public bool  isAOEItem        = false; // If true, all attacks trigger an AOE explosion

    [TextArea(3, 5)]
    public string description;
}

public enum ItemRarity
{
    Common,    // White
    Uncommon,  // Green
    Rare       // Red
}
