using UnityEngine;

// PREFAB SETUP:
// 1. Create empty GameObject "HealPickup"
// 2. Add SpriteRenderer — use a green cross or heart sprite
// 3. Add CircleCollider2D — Is Trigger = true, radius 0.3
// 4. Add this script
// 5. Save as prefab — assign to DropManager healPickupPrefab slot

public class HealPickup : MonoBehaviour
{
    [SerializeField] private float healPercent  = 0.15f; // 15% of max HP
    [SerializeField] private float magnetRange  = 5f;
    [SerializeField] private float magnetSpeed  = 12f;
    [SerializeField] private float bobSpeed     = 2f;
    [SerializeField] private float bobHeight    = 0.2f;

    private Transform player;
    private Vector3   startPos;

    private void Start()
    {
        player   = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPos = transform.position;
        Destroy(gameObject, 30f);
    }

    private void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= magnetRange)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, player.position, magnetSpeed * Time.deltaTime);
        }
        else
        {
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth ph = collision.GetComponent<PlayerHealth>();
        if (ph != null)
        {
            float healAmount = ph.MaxHealth * healPercent;
            ph.Heal(healAmount);
            Debug.Log($"[HealPickup] Healed {healAmount:F1} HP ({healPercent * 100f}% of max)");
        }

        Destroy(gameObject);
    }
}
