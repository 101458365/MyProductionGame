using UnityEngine;
using TMPro;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI goldText;

    private int currentGold = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (currentGold < amount) return false;
        currentGold -= amount;
        UpdateUI();
        return true;
    }

    public bool HasEnough(int amount) => currentGold >= amount;

    private void UpdateUI()
    {
        if (goldText != null)
            goldText.text = $"Gold: {currentGold}";
    }

    public int CurrentGold => currentGold;
}
