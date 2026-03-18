using UnityEngine;

public class GoldPickup : MonoBehaviour
{
    [SerializeField] private int goldAmount = 3;
    [SerializeField] private float magnetRange = 6f;
    [SerializeField] private float magnetSpeed = 14f;

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        Destroy(gameObject, 30f);
    }

    private void Update()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= magnetRange)
            transform.position = Vector3.MoveTowards(transform.position, player.position, magnetSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (GoldManager.Instance != null)
            GoldManager.Instance.AddGold(goldAmount);

        Destroy(gameObject);
    }

    public void SetGoldAmount(int amount) => goldAmount = amount;
}
