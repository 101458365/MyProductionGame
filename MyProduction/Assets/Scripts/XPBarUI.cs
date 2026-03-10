using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider xpBar;
    [SerializeField] private TextMeshProUGUI levelText;

    private PlayerLevel playerLevel;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerLevel = player.GetComponent<PlayerLevel>();
        }
    }

    private void Update()
    {
        if (playerLevel != null)
        {
            UpdateXPBar();
        }
    }

    private void UpdateXPBar()
    {
        // Update XP bar progress
        if (xpBar != null)
        {
            xpBar.value = playerLevel.XPProgress;
        }

        // Update level text
        if (levelText != null)
        {
            levelText.text = $"Lv.{playerLevel.CurrentLevel}";
        }
    }
}