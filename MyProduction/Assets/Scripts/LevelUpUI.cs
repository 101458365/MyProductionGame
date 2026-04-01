using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUpUI : MonoBehaviour
{
    public static LevelUpUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject levelUpPanel;
    [SerializeField] private Button     choice1Button;
    [SerializeField] private Button     choice2Button;
    [SerializeField] private Button     choice3Button;

    [SerializeField] private TextMeshProUGUI choice1Text;
    [SerializeField] private TextMeshProUGUI choice2Text;
    [SerializeField] private TextMeshProUGUI choice3Text;

    [Header("Available Upgrades")]
    [SerializeField] private StatUpgrade[] allUpgrades;

    private StatUpgrade currentChoice1;
    private StatUpgrade currentChoice2;
    private StatUpgrade currentChoice3;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);
    }

    public void ShowLevelUpScreen()
    {
        Time.timeScale = 0f;

        currentChoice1 = GetRandomUpgrade();
        currentChoice2 = GetRandomUpgrade();
        currentChoice3 = GetRandomUpgrade();

        if (choice1Text != null)
            choice1Text.text = $"{currentChoice1.upgradeName}\n<size=18>{currentChoice1.description}</size>";
        if (choice2Text != null)
            choice2Text.text = $"{currentChoice2.upgradeName}\n<size=18>{currentChoice2.description}</size>";
        if (choice3Text != null)
            choice3Text.text = $"{currentChoice3.upgradeName}\n<size=18>{currentChoice3.description}</size>";

        if (levelUpPanel != null)
            levelUpPanel.SetActive(true);
    }

    public void OnChoice1Selected() { ApplyUpgrade(currentChoice1); CloseLevelUpScreen(); }
    public void OnChoice2Selected() { ApplyUpgrade(currentChoice2); CloseLevelUpScreen(); }
    public void OnChoice3Selected() { ApplyUpgrade(currentChoice3); CloseLevelUpScreen(); }

    private void ApplyUpgrade(StatUpgrade upgrade)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerLevel playerLevel = player.GetComponent<PlayerLevel>();
        if (playerLevel == null) return;

        switch (upgrade.type)
        {
            case UpgradeType.MaxHealth:
                playerLevel.IncreaseMaxHealth(upgrade.value);
                break;
            case UpgradeType.Damage:
                playerLevel.IncreaseDamage(upgrade.value);
                break;
            case UpgradeType.AttackSpeed:
                playerLevel.IncreaseAttackSpeed(upgrade.value);
                break;
            case UpgradeType.MoveSpeed:
                playerLevel.IncreaseMoveSpeed(upgrade.value);
                break;
            case UpgradeType.ProjectileCount:
                playerLevel.IncreaseProjectileCount((int)upgrade.value);
                break;
            case UpgradeType.Dismantle:
                playerLevel.IncreaseDismantle((int)upgrade.value);
                break;
        }

        Debug.Log($"Applied upgrade: {upgrade.upgradeName}");
    }

    private StatUpgrade GetRandomUpgrade()
    {
        if (allUpgrades.Length == 0) return null;
        return allUpgrades[Random.Range(0, allUpgrades.Length)];
    }

    private void CloseLevelUpScreen()
    {
        if (levelUpPanel != null)
            levelUpPanel.SetActive(false);

        Time.timeScale = 1f;
    }
}
