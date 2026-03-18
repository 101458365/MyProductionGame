using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [Header("Child References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;

    public void Initialize(Sprite icon, int count, Color rarityColor, int size, bool showWhenOne)
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = new Vector2(size, size);

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (backgroundImage != null)
            backgroundImage.color = rarityColor;

        SetCount(count, showWhenOne);
    }

    public void SetCount(int count, bool showWhenOne)
    {
        if (countText == null) return;

        if (count <= 1 && !showWhenOne)
        {
            countText.gameObject.SetActive(false);
        }
        else
        {
            countText.gameObject.SetActive(true);
            countText.text = $"x{count}";
        }
    }
}
