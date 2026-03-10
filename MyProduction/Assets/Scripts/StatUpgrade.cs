using UnityEngine;

[System.Serializable]
public class StatUpgrade
{
    public string upgradeName;
    public string description;
    public UpgradeType type;
    public float value;
}

public enum UpgradeType
{
    MaxHealth,
    Damage,
    AttackSpeed,
    MoveSpeed,
    ProjectileCount
}